using System.Reflection;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 完全掌握 — このターン、攻撃ダメージの着弾先を所有者（プレイヤー）へ寄せる。
/// 重ねがけで残り敵ターン+1。
/// </summary>
public class TotalControlPower : HypnosisCreatorPower
{
    private static readonly FieldInfo? SingleTargetField =
        typeof(AttackCommand).GetField("_singleTarget", BindingFlags.Instance | BindingFlags.NonPublic);

    private int _remainingEnemyTurns;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _remainingEnemyTurns = TurnScopedDuration.AddStack(_remainingEnemyTurns);
        return Task.CompletedTask;
    }

    public override Creature ModifyUnblockedDamageTarget(
        Creature target, decimal amount, ValueProp props, Creature dealer)
    {
        if (Owner is { IsAlive: true })
            return Owner;
        return target;
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        if (Owner is not { IsAlive: true }) return Task.CompletedTask;
        try
        {
            SingleTargetField?.SetValue(command, Owner);
        }
        catch
        {
            // リダイレクト不能時は ModifyUnblockedDamageTarget 側に委ねる
        }

        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (side != CombatSide.Enemy) return;
        if (!participants.Contains(Owner)) return;
        if (!TurnScopedDuration.Consume(ref _remainingEnemyTurns))
            return;
        await PowerCmd.Remove(this);
    }
}
