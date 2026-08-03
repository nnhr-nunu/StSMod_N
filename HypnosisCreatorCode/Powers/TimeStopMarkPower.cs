using HypnosisCreator.HypnosisCreatorCode.Utils;
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

        var stackedBase = Amount;
        var dealer = TimeStopStrikeDamage.ResolveDealer(Applier, Owner);
        var combat = Owner.CombatState;
        var runState = dealer?.Player?.RunState;
        await PowerCmd.Remove(this);
        if (stackedBase <= 0 || dealer == null || combat == null || runState == null) return;

        var dmg = TimeStopStrikeDamage.ResolveTurnEnd(runState, combat, Owner, dealer, stackedBase);
        if (dmg <= 0) return;

        if (dealer.Player != null
            && FingerSnapCardRules.IsHypnosisCreatorPlayer(dealer.Player))
        {
            VanillaAttackSfx.PlayStrike();
        }

        // 弱体・感度3000倍等は上で反映済み。二重適用を避けるため Unpowered で与える。
        await CreatureCmd.Damage(
            choiceContext,
            Owner,
            dmg,
            ValueProp.Unpowered,
            dealer,
            null,
            null);
    }
}
