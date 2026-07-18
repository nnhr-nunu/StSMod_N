using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 脳くちゅ催眠 — 所有者（敵）の攻撃着弾を、味方を除くランダムな他敵へ寄せる。
/// </summary>
public class BrainSlimeRedirectPower : HypnosisCreatorPower
{
    private static readonly FieldInfo? SingleTargetField =
        typeof(AttackCommand).GetField("_singleTarget", BindingFlags.Instance | BindingFlags.NonPublic);

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override Creature ModifyUnblockedDamageTarget(
        Creature target, decimal amount, ValueProp props, Creature dealer)
    {
        if (Owner == null || dealer != Owner || CombatState == null) return target;
        var retarget = PickRetarget();
        return retarget ?? target;
    }

    public override Task BeforeAttack(AttackCommand command)
    {
        if (Owner == null || command.Attacker != Owner) return Task.CompletedTask;
        var retarget = PickRetarget();
        if (retarget == null) return Task.CompletedTask;
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

    private Creature? PickRetarget()
    {
        if (Owner == null || CombatState == null) return null;
        var candidates = CombatState.HittableEnemies.Where(e => e != Owner && e.IsAlive).ToList();
        if (candidates.Count == 0) return null;
        var rng = Owner.Monster?.CombatState?.Players.FirstOrDefault()?.RunState.Rng.CombatCardSelection
                  ?? Owner.Player?.RunState.Rng.CombatCardSelection;
        if (rng == null) return candidates[0];
        return candidates[rng.NextInt(candidates.Count)];
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> participants)
    {
        // 永続寄り: 戦闘中維持。必要なら敵ターン後に消す場合はここで Remove。
        await Task.CompletedTask;
    }
}
