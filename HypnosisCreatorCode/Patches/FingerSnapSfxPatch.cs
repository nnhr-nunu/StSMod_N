using System.Collections;
using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands.Builders;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// ヒプノクリエイターの攻撃ヒット時に指パッチン SE を鳴らし、vanilla の剣／鈍撃音は抑止する。
/// </summary>
[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.Execute))]
public static class FingerSnapSuppressVanillaHitSfxPatch
{
    private static readonly FieldInfo? HitSfxField =
        AccessTools.Field(typeof(AttackCommand), "<HitSfx>k__BackingField");

    private static readonly FieldInfo? TmpHitSfxField =
        AccessTools.Field(typeof(AttackCommand), "<TmpHitSfx>k__BackingField");

    public static void Prefix(AttackCommand __instance)
    {
        if (!FingerSnapCardRules.IsHypnosisCreatorPlayer(__instance.Attacker?.Player)) return;
        HitSfxField?.SetValue(__instance, "");
        TmpHitSfxField?.SetValue(__instance, "");
    }
}

[HarmonyPatch(typeof(AttackContext), nameof(AttackContext.AddHit))]
public static class FingerSnapAttackHitSfxPatch
{
    private static readonly FieldInfo? AttackCommandField =
        AccessTools.Field(typeof(AttackContext), "_attackCommand");

    private static readonly FieldInfo? HitCountField =
        AccessTools.Field(typeof(AttackCommand), "_hitCount");

    private static readonly FieldInfo? ResultsField =
        AccessTools.Field(typeof(AttackCommand), "_results");

    public static void Postfix(AttackContext __instance)
    {
        if (AttackCommandField?.GetValue(__instance) is not AttackCommand cmd) return;
        if (!FingerSnapCardRules.IsHypnosisCreatorPlayer(cmd.Attacker?.Player)) return;

        var hitCount = HitCountField?.GetValue(cmd) switch
        {
            int i => i,
            _ => 1
        };
        var hitIndex = (ResultsField?.GetValue(cmd) as IList)?.Count ?? 1;
        if (hitIndex <= 0) return;

        FingerSnapSfx.PlayForHit(hitCount, hitIndex);
    }
}
