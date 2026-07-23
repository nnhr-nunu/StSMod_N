using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 強制睡眠中は攻撃意図の代わりに睡眠アイコンを表示する（NextMove は変更しない）。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.UpdateIntent))]
public static class SleepIntentDisplayPatch
{
    public static bool Prefix(NCreature __instance, IEnumerable<Creature> targets, ref Task __result)
    {
        if (!SleepIntentPresentation.ShouldOverride(__instance.Entity))
            return true;

        __result = SleepIntentPresentation.UpdateIntent(__instance, targets);
        return false;
    }
}
