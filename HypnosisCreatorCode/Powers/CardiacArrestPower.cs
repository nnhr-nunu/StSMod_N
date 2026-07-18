using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 心停止催眠 — 残りターン数を毎プレイヤーターン開始時に1減らし、0になると即死させる。
/// </summary>
public class CardiacArrestPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (Owner == null || !Owner.IsAlive) return;

        var target = Owner;
        await PowerCmd.Decrement(this);
        if (target.GetPower<CardiacArrestPower>() == null || target.GetPowerAmount<CardiacArrestPower>() <= 0)
            await CreatureCmd.Kill(target);
    }
}
