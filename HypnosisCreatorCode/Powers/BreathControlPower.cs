using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// このターン、カードがプレイされるたび所有者（敵）の筋力を1失わせる。
/// 攻撃意図の値が0ならスタン。
/// </summary>
public class BreathControlPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;

        await PowerCmd.Apply<StrengthPower>(
            choiceContext, Owner, -1m, Applier ?? Owner, cardPlay.Card);

        if (IsAttackValueZero(Owner))
            await CreatureCmd.Stun(Owner);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (side != CombatSide.Player) return;
        await PowerCmd.Remove(this);
    }

    private static bool IsAttackValueZero(Creature enemy) =>
        EnemyAttackIntents.IntendsToAttack(enemy) &&
        EnemyAttackIntents.GetTotalDamage(enemy) <= 0;
}
