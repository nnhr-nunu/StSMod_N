using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 認知シャッフル — 対象がトランス中のプレイヤーターン開始時、選んだ形態と同プールのカードを生成する。
/// 生成は手札ドロー前（<see cref="BeforeHandDraw"/>）。トランス減少後の Late で追跡対象が全員トランス切れなら見た目を戻して自己解除する。
/// 集団催眠で複数敵へ波及した場合は、いずれかがトランス中なら継続する。
/// </summary>
public class CognitiveShufflePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>生成元となる形態カードのカノニカル。</summary>
    public CardModel? FormCanonical { get; set; }

    private readonly List<Creature> _tranceTargets = [];

    private CharacterDisguise.State? _disguise;

    public void TrackTranceTarget(Creature target)
    {
        if (target is not { IsAlive: true, IsEnemy: true }) return;
        if (!_tranceTargets.Contains(target))
            _tranceTargets.Add(target);
    }

    public void ApplyDisguise(CharacterModel character)
    {
        if (Owner == null) return;
        _disguise = CharacterDisguise.Apply(Owner, character);
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

        await GenerateMatchingCardsAsync(player);
    }

    public override async Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (Owner == null || !participants.Contains(Owner)) return;

        PruneDeadTargets();

        // トランス減少後: 追跡対象の誰もトランス中でなければ見た目復帰＋自己解除
        if (_tranceTargets.Count == 0 || !_tranceTargets.Any(TranceCombat.HasTrance))
            await ExpireAsync();
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

    public override Task AfterRemoved(Creature oldOwner)
    {
        CharacterDisguise.Restore(oldOwner, _disguise);
        _disguise = null;
        _tranceTargets.Clear();
        return Task.CompletedTask;
    }

    private void PruneDeadTargets() =>
        _tranceTargets.RemoveAll(t => t is not { IsAlive: true, IsEnemy: true });

    private async Task ExpireAsync()
    {
        if (Owner != null)
            CharacterDisguise.Restore(Owner, _disguise);
        _disguise = null;
        _tranceTargets.Clear();
        await PowerCmd.Remove(this);
    }
}
