using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 剥き出しの本能 — 自ターン開始時、手札のランダムな攻撃カードへ「本能」（強化攻撃×2）を付与する。
/// 付与数はスタック（Amount）で管理し、同名カードの再プレイで増える。
/// </summary>
public class BareInstinctPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner == null || !participants.Contains(Owner)) return Task.CompletedTask;

        var player = Owner.Player;
        var hand = player?.PlayerCombatState?.Hand;
        if (hand == null) return Task.CompletedTask;

        var candidates = hand.Cards
            .Where(c => c.Type == CardType.Attack && c.Enchantment == null)
            .ToList();
        if (candidates.Count == 0) return Task.CompletedTask;

        var rng = player!.RunState.Rng.CombatCardSelection;
        var pickCount = Math.Min(Amount, candidates.Count);
        for (var i = 0; i < pickCount; i++)
        {
            var index = rng.NextInt(candidates.Count);
            var card = candidates[index];
            candidates.RemoveAt(index);
            CardCmd.Enchant<Instinct>(card, 1);
        }

        return Task.CompletedTask;
    }
}
