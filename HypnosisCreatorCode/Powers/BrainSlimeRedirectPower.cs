using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 脳くちゅ催眠 — 所有者（敵）の攻撃を他敵へ寄せる。1ターンのみ。プレイヤーには当てない。
/// </summary>
public class BrainSlimeRedirectPower : HypnosisCreatorPower
{
    private static readonly FieldInfo? SingleTargetField =
        typeof(AttackCommand).GetField("_singleTarget", BindingFlags.Instance | BindingFlags.NonPublic);

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>現在のリダイレクト先。死亡などで無効なら再抽選。</summary>
    public Creature? LockedTarget { get; private set; }

    public override LocString Description
    {
        get
        {
            var loc = base.Description;
            loc.Add("Target", TargetDisplayName());
            return loc;
        }
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        EnsureLockedTarget(forceReroll: true);
        return Task.CompletedTask;
    }

    public override Creature ModifyUnblockedDamageTarget(
        Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        if (Owner == null || dealer != Owner || CombatState == null) return target;
        return ResolveRetarget();
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        if (Owner == null || command.Attacker != Owner) return Task.CompletedTask;
        var retarget = ResolveRetarget();
        try
        {
            SingleTargetField?.SetValue(command, retarget);
        }
        catch
        {
            // best-effort
        }

        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || !Owner.IsAlive) return;
        // 敵側ターン終了で消滅（1ターン継続）
        if (side != Owner.Side) return;
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }

    private Creature ResolveRetarget()
    {
        EnsureLockedTarget(forceReroll: false);
        if (LockedTarget is { IsAlive: true, IsEnemy: true } && LockedTarget != Owner)
            return LockedTarget;

        EnsureLockedTarget(forceReroll: true);
        if (LockedTarget is { IsAlive: true, IsEnemy: true } && LockedTarget != Owner)
            return LockedTarget;

        // 他に敵がいない: プレイヤーへは当てず、自分を殴る
        return Owner!;
    }

    private void EnsureLockedTarget(bool forceReroll)
    {
        if (!forceReroll && LockedTarget is { IsAlive: true, IsEnemy: true } && LockedTarget != Owner)
            return;

        LockedTarget = PickNewTarget();
    }

    private Creature? PickNewTarget()
    {
        if (Owner == null || CombatState == null) return null;
        var candidates = CombatState.HittableEnemies
            .Where(e => e != Owner && e.IsAlive)
            .ToList();
        if (candidates.Count == 0) return null;

        var rng = Owner.Monster?.CombatState?.Players.FirstOrDefault()?.RunState.Rng.CombatCardSelection
                  ?? Owner.Player?.RunState.Rng.CombatCardSelection;
        if (rng == null) return candidates[0];
        return candidates[rng.NextInt(candidates.Count)];
    }

    private string TargetDisplayName()
    {
        EnsureLockedTarget(forceReroll: false);
        if (LockedTarget is { IsAlive: true })
            return LockedTarget.Name;
        return "—";
    }
}
