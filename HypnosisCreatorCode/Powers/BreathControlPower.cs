using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// このターン、カードがプレイされるたび所有者（敵）の筋力を Amount 失う。
/// 呼吸制御カード自体のプレイは対象外。攻撃意図の値が0ならスタン。
/// 筋力低下は敵ターン中も維持し、次のプレイヤーターン開始時に戻す。重ねがけで Amount+1。
/// </summary>
public class BreathControlPower : HypnosisCreatorPower
{
    private int _strengthDrained;
    private bool _drainExpired;
    private bool _restorePending;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_drainExpired) return;
        if (Owner == null || !Owner.IsAlive) return;
        if (cardPlay.Card is BreathControl) return;

        var loss = Math.Max(1, Amount);
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, Owner, -loss, Applier ?? Owner, cardPlay.Card);
        _strengthDrained += loss;

        if (IsAttackValueZero(Owner, cardPlay.Card.Owner))
            await CreatureCmd.Stun(Owner);
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return Task.CompletedTask;

        _drainExpired = true;
        _restorePending = true;
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (!_restorePending) return;

        if (Owner is { IsAlive: true } && _strengthDrained > 0)
        {
            var ctx = new ThrowingPlayerChoiceContext();
            await PowerCmd.Apply<StrengthPower>(
                ctx, Owner, _strengthDrained, Applier ?? Owner, null!);
        }

        await PowerCmd.Remove(this);
    }

    private static bool IsAttackValueZero(Creature enemy, Player? perspectivePlayer) =>
        EnemyAttackIntents.IntendsToAttack(enemy)
        && EnemyAttackIntents.GetTotalDamage(enemy, perspectivePlayer) <= 0;
}
