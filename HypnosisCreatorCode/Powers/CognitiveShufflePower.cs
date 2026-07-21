using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 認知シャッフル — 対象がトランス中のプレイヤーターン開始時、選んだ形態と同プールのカードを生成する。
/// トランス減少（AfterSideTurnStart）のあと Late で判定し、0 なら見た目を戻して自己解除する。
/// </summary>
public class CognitiveShufflePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>生成元となる形態カードのカノニカル。</summary>
    public CardModel? FormCanonical { get; set; }

    /// <summary>トランス継続を見る対象敵。</summary>
    public Creature? TranceTarget { get; set; }

    private CharacterDisguise.State? _disguise;

    public void ApplyDisguise(CharacterModel character)
    {
        if (Owner == null) return;
        _disguise = CharacterDisguise.Apply(Owner, character);
    }

    public override async Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (Owner == null || !participants.Contains(Owner)) return;

        // トランス減少後: 0 なら見た目復帰＋自己解除（次ターン開始時点で既に無い）
        if (TranceTarget == null || !TranceTarget.IsAlive || !TranceCombat.HasTrance(TranceTarget))
        {
            await ExpireAsync();
            return;
        }

        if (FormCanonical?.Pool == null || CombatState == null) return;

        var player = Owner.Player;
        if (player == null) return;

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
        return Task.CompletedTask;
    }

    private async Task ExpireAsync()
    {
        if (Owner != null)
            CharacterDisguise.Restore(Owner, _disguise);
        _disguise = null;
        await PowerCmd.Remove(this);
    }
}
