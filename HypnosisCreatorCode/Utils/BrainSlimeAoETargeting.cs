using System.Reflection;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>脳くちゅ催眠 UG — 指定敵の攻撃を相手全員へ飛ばす（ダメージ・付随デバフ含む）。</summary>
internal static class BrainSlimeAoETargeting
{
    private static readonly FieldInfo? CombatStateField =
        typeof(AttackCommand).GetField("_combatState", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? SingleTargetField =
        typeof(AttackCommand).GetField("_singleTarget", BindingFlags.Instance | BindingFlags.NonPublic);

    public static bool TryEnable(AttackCommand command, BrainSlimeRedirectPower power)
    {
        if (!power.RedirectAllEnemies || power.Owner == null) return false;
        if (command.Attacker != power.Owner) return false;
        if (!AttackWouldHitPlayer(command)) return false;
        if (power.CombatState == null) return false;

        SingleTargetField?.SetValue(command, null);
        CombatStateField?.SetValue(command, power.CombatState);
        return true;
    }

    public static bool ShouldReplaceTargets(AttackCommand command) =>
        command.IsMultiTargeted
        && command.Attacker?.GetPower<BrainSlimeRedirectPower>() is { RedirectAllEnemies: true };

    private static Creature? GetSingleTarget(AttackCommand command) =>
        SingleTargetField?.GetValue(command) as Creature;

    private static bool AttackWouldHitPlayer(AttackCommand command)
    {
        var single = GetSingleTarget(command);
        if (single is { IsPlayer: true }) return true;

        var attacker = command.Attacker;
        if (attacker == null) return false;

        if (!command.IsMultiTargeted && !command.IsRandomlyTargeted) return false;

        return attacker.CombatState?.GetOpponentsOf(attacker).Any(c => c.IsPlayer) == true;
    }
}
