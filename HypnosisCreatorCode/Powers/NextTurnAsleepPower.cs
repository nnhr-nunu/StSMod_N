using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>寝不足 — 次のプレイヤーターン開始時に睡眠を付与して消える。</summary>
public class NextTurnAsleepPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (side != CombatSide.Player) return;

        var amount = Amount;
        var target = Owner;
        var ctx = new ThrowingPlayerChoiceContext();
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<AsleepPower>(ctx, target, amount, target, null);
        await ForcedSleep.EnsurePresentation(ctx, target, target, cardSource: null);
    }
}
