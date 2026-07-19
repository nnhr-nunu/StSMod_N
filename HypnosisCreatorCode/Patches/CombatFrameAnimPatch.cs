using System.Reflection;
using System.Threading.Tasks;
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
/// Spine が無いキャラはアニメ長が 0 になるため、連番 SpriteFrames の長さを返す。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.GetCurrentAnimationLength))]
public static class CombatFrameAnimLengthPatch
{
    public static void Postfix(NCreature __instance, ref double __result)
    {
        if (__result > 0) return;
        var len = CombatFrameAnimator.GetCurrentAnimLengthSeconds(__instance);
        if (len > 0)
            __result = len;
    }
}

/// <summary>
/// WithHitCount による多段ヒット中は攻撃モーションをループ再生する。
/// Execute は async のため Finalizer では待てず、Postfix で Task を包む。
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
        var hitCount = ReadHitCount(__instance);
        if (hitCount <= 1) return;

        var attacker = ResolveAttacker(__instance);
        if (attacker == null) return;

        // カード側で既にループ中なら、Execute 単位で止めない
        if (CombatFrameAnimator.IsAttackLoopActive(attacker)) return;

        CombatFrameAnimator.BeginAttackLoop(attacker);
        __state = true;
    }

    public static void Postfix(AttackCommand __instance, ref Task<AttackCommand> __result, bool __state)
    {
        if (!__state) return;
        var attacker = ResolveAttacker(__instance);
        var original = __result;
        __result = AwaitThenEndLoop(original, attacker);
    }

    private static async Task<AttackCommand> AwaitThenEndLoop(
        Task<AttackCommand> original, Creature? attacker)
    {
        try
        {
            return await original;
        }
        finally
        {
            CombatFrameAnimator.EndAttackLoop(attacker);
        }
    }

    private static int ReadHitCount(AttackCommand cmd)
    {
        // boxed int を `as int?` すると常に null になるため変換する
        var raw = HitCountField?.GetValue(cmd);
        return raw switch
        {
            int i => i,
            _ => 1
        };
    }

    private static Creature? ResolveAttacker(AttackCommand cmd)
    {
        if (VisualAttackerField?.GetValue(cmd) is Creature visual)
            return visual;
        return cmd.Attacker;
    }
}
