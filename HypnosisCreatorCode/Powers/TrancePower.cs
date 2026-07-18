using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 敵のトランス。プレイヤーターン開始（Player side の SideTurnStart）に1減少。
/// mechanics-lock.md 参照。
/// </summary>
public class TrancePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner == null || !Owner.IsAlive || !Owner.IsEnemy) return;
        // プレイヤー側のターンが始まったときに減少（マルチでも side 単位で1回）
        if (side == Owner.Side) return;
        if (Amount <= 0) return;
        await PowerCmd.Decrement(this);
    }
}
