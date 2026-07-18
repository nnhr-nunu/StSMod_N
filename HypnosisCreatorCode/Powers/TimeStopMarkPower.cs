using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>時止めストライク — 蓄積したダメージをプレイヤーターン終了時にまとめて与える。</summary>
public class TimeStopMarkPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (Owner == null || !Owner.IsAlive) return;

        var dmg = Amount;
        await PowerCmd.Remove(this);
        if (dmg > 0)
            await CreatureCmd.Damage(choiceContext, Owner, dmg, ValueProp.Move, Applier);
    }
}
