using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// Spine 未使用の連番立ち絵向けに、ゲームのアニメリクエストを AnimatedSprite2D へ橋渡しする。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
public static class CombatFrameAnimTriggerPatch
{
    public static void Postfix(NCreature __instance, string trigger) =>
        CombatFrameAnimator.TryPlay(__instance, trigger);
}

[HarmonyPatch(typeof(NCreature), "ImmediatelySetIdle")]
public static class CombatFrameAnimIdlePatch
{
    public static void Postfix(NCreature __instance) =>
        CombatFrameAnimator.TryIdle(__instance);
}

/// <summary>
/// WithHitCount による多段ヒット中は攻撃モーションをループ再生する。
/// </summary>
[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.Execute))]
public static class AttackCommandMultiHitAnimPatch
{
    private static readonly FieldInfo? HitCountField =
        AccessTools.Field(typeof(AttackCommand), "_hitCount");

    private static readonly FieldInfo? VisualAttackerField =
        AccessTools.Field(typeof(AttackCommand), "_visualAttacker");

    public static void Prefix(AttackCommand __instance, out bool __state)
    {
        __state = false;
        var hitCount = HitCountField?.GetValue(__instance) as int? ?? 1;
        if (hitCount <= 1) return;

        var attacker = ResolveAttacker(__instance);
        if (attacker == null) return;

        // カード側で既にループ中なら、Execute 単位で止めない
        if (CombatFrameAnimator.IsAttackLoopActive(attacker)) return;

        CombatFrameAnimator.BeginAttackLoop(attacker);
        __state = true;
    }

    public static void Finalizer(AttackCommand __instance, bool __state)
    {
        if (!__state) return;
        var attacker = ResolveAttacker(__instance);
        CombatFrameAnimator.EndAttackLoop(attacker);
    }

    private static Creature? ResolveAttacker(AttackCommand cmd)
    {
        if (VisualAttackerField?.GetValue(cmd) is Creature visual && visual != null)
            return visual;
        return cmd.Attacker;
    }
}
