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
/// </summary>
public class CognitiveShufflePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>生成元となる形態カードのカノニカル。</summary>
    public CardModel? FormCanonical { get; set; }

    /// <summary>トランス継続を見る対象敵。</summary>
    public Creature? TranceTarget { get; set; }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (Owner == null || !participants.Contains(Owner)) return;
        if (TranceTarget == null || !TranceCombat.HasTrance(TranceTarget)) return;
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
}
