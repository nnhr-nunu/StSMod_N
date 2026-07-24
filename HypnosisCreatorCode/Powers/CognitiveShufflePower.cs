using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 認知シャッフル — 対象がトランス中のプレイヤーターン開始時、選んだ形態と同プールのカードを生成する。
/// 生成は手札ドロー前（<see cref="BeforeHandDraw"/>）。追跡対象のトランスが2以上のときだけ生成（トランス5なら4回）。
/// 追跡対象のトランスが全て切れた／敵が倒れたら見た目復帰＋形態パワー解除＋自己解除。
/// 集団催眠で複数敵へ波及した場合は、いずれかがトランス中なら継続する。
/// </summary>
public class CognitiveShufflePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>生成元となる形態カードのカノニカル。</summary>
    public CardModel? FormCanonical { get; set; }

    private readonly List<Creature> _tranceTargets = [];
    private readonly List<PowerModel> _grantedForms = [];

    private CharacterModel? _disguiseCharacter;
    private CharacterDisguise.State? _disguise;
    private bool _expiring;
    private bool _pendingExpire;

    public void SetDisguiseCharacter(CharacterModel character) => _disguiseCharacter = character;

    public override string CustomPackedIconPath =>
        _disguiseCharacter != null
            ? CognitiveCharacterFaces.CharacterSelectIconPath(_disguiseCharacter)
            : base.CustomPackedIconPath;

    public override string CustomBigIconPath =>
        _disguiseCharacter != null
            ? CognitiveCharacterFaces.CharacterSelectIconPath(_disguiseCharacter)
            : base.CustomBigIconPath;

    public void RegisterGrantedForm(PowerModel? power)
    {
        if (power != null)
            _grantedForms.Add(power);
    }

    public override Task BeforeApplied(Creature target, decimal amount, Creature applier, CardModel cardSource)
    {
        _ = amount;
        _ = applier;
        _ = cardSource;
        var character = CognitiveShuffleApplyContext.TakeIconCharacter(target.Player);
        if (character != null)
            SetDisguiseCharacter(character);
        return base.BeforeApplied(target, amount, applier, cardSource);
    }

    public void TrackTranceTarget(Creature target)
    {
        if (target is not { IsAlive: true, IsEnemy: true }) return;
        if (!_tranceTargets.Contains(target))
            _tranceTargets.Add(target);
    }

    public void ApplyDisguise()
    {
        if (Owner == null || _disguiseCharacter == null) return;
        _disguise = CharacterDisguise.Apply(Owner, _disguiseCharacter);
    }

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        _ = choiceContext;
        _ = combatState;
        if (Owner == null || player.Creature != Owner) return;

        PruneDeadTargets();
        if (_tranceTargets.Count == 0 || !_tranceTargets.Any(TranceCombat.HasTrance)) return;
        // 残り1のターンは生成しない（減少後に見た目が戻るため、他色カードだけ残るのを防ぐ）
        if (!_tranceTargets.Any(t => TranceCombat.GetTrance(t) > 1)) return;

        await GenerateMatchingCardsAsync(player);
    }

    public override async Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (Owner == null || !participants.Contains(Owner)) return;

        ScheduleExpireIfNeeded();
        if (_pendingExpire && !_expiring)
            await ExpireAsync(new ThrowingPlayerChoiceContext());
    }

    /// <summary>
    /// カードプレイ完了後に期限切れ（暗示解除の OnPlay 中は PowerCmd 競合するためここで実行）。
    /// </summary>
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Card.Owner?.Creature != Owner) return;
        if (!_pendingExpire || _expiring) return;
        await ExpireAsync(choiceContext);
    }

    /// <summary>トランス解除・敵死亡時に全プレイヤーの認知シャッフルを確認する。</summary>
    public static void NotifyTranceTargetChanged(Creature? target)
    {
        _ = target;
        var combat = target?.CombatState;
        if (combat == null) return;

        foreach (var player in combat.Players)
            player.Creature?.GetPower<CognitiveShufflePower>()?.ScheduleExpireIfNeeded();
    }

    private void ScheduleExpireIfNeeded()
    {
        if (_expiring) return;

        PruneDeadTargets();
        if (_tranceTargets.Count == 0 || !_tranceTargets.Any(TranceCombat.HasTrance))
            _pendingExpire = true;
    }

    private async Task GenerateMatchingCardsAsync(Player player)
    {
        if (FormCanonical?.Pool == null || CombatState == null) return;

        var poolType = FormCanonical.Pool.GetType();
        var candidates = ModelDb.AllCards
            .Where(c =>
                c.Pool?.GetType() == poolType &&
                c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare &&
                c.Type is not CardType.Status and not CardType.Curse &&
                c.GetType() != FormCanonical.GetType())
            .ToList();
        if (candidates.Count == 0) return;

        var rng = player.RunState.Rng.CombatCardSelection;
        var count = Math.Max(1, Amount);
        for (var i = 0; i < count; i++)
        {
            var canonical = candidates[rng.NextInt(candidates.Count)];
            var generated = CombatState.CreateCard(canonical, player);
            generated.AddKeyword(CardKeyword.Ethereal);
            generated.AddKeyword(CardKeyword.Exhaust);
            generated.EnergyCost.SetThisTurn(0);
            await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, player);
        }
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        CharacterDisguise.Restore(oldOwner, _disguise);
        _disguise = null;
        _tranceTargets.Clear();
        _grantedForms.Clear();
        await base.AfterRemoved(oldOwner);
    }

    private void PruneDeadTargets() =>
        _tranceTargets.RemoveAll(t => t is not { IsAlive: true, IsEnemy: true });

    private async Task ExpireAsync(PlayerChoiceContext choiceContext)
    {
        _ = choiceContext;
        if (_expiring || Owner == null || !_pendingExpire) return;

        _pendingExpire = false;
        _expiring = true;

        try
        {
            CharacterDisguise.Restore(Owner, _disguise);
            _disguise = null;
            _tranceTargets.Clear();
            await RemoveGrantedFormsAsync();
            await PowerCmd.Remove(this);
        }
        finally
        {
            _expiring = false;
        }
    }

    private async Task RemoveGrantedFormsAsync()
    {
        foreach (var power in _grantedForms.ToList())
            await PowerCmd.Remove(power);

        _grantedForms.Clear();
    }
}
