using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
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
