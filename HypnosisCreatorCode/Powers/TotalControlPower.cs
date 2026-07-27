using System;
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
/// 完全掌握 — このターン、プレイヤーに向かう攻撃ダメージを所有者（掌握された敵）が肩代わりする。
/// 締め付けなど非攻撃ダメージ・敵への攻撃は対象外。重ねがけで残り敵ターン+1。
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
        if (Owner is not { IsAlive: true }) return target;
        if (!props.IsPoweredAttack()) return target;
        if (target is not { IsPlayer: true }) return target;
        return Owner;
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        if (Owner is not { IsAlive: true }) return Task.CompletedTask;

        // 単体敵戦：掌握対象自身の攻撃は TargetSide 経路でも自分へ向ける（脳くちゅ催眠と同型）
        if (command.Attacker == Owner && AttackWouldHitPlayer(command))
        {
            TrySetSingleTarget(command, Owner);
            return Task.CompletedTask;
        }

        // _singleTarget が明示されている単体プレイヤー攻撃（他敵の肩代わり）
        if (GetSingleTarget(command) is { IsPlayer: true })
            TrySetSingleTarget(command, Owner);

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

    private static Creature? GetSingleTarget(AttackCommand command) =>
        SingleTargetField?.GetValue(command) as Creature;

    /// <summary>
    /// 本家 <see cref="AttackCommand.Targeting(Creature)"/> は <c>_singleTarget</c> を立てず
    /// <c>TargetSide</c> だけ更新するため、<c>IsMultiTargeted</c> 経路も見る。
    /// </summary>
    private static bool AttackWouldHitPlayer(AttackCommand command)
    {
        var single = GetSingleTarget(command);
        if (single is { IsPlayer: true }) return true;

        var attacker = command.Attacker;
        if (attacker == null) return false;

        if (!command.IsMultiTargeted && !command.IsRandomlyTargeted) return false;

        return attacker.CombatState?.GetOpponentsOf(attacker).Any(c => c.IsPlayer) == true;
    }

    private static void TrySetSingleTarget(AttackCommand command, Creature? target)
    {
        if (target == null) return;
        try
        {
            SingleTargetField?.SetValue(command, target);
        }
        catch
        {
            // リダイレクト不能時は ModifyUnblockedDamageTarget 側に委ねる
        }
    }
}
