using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// マシュマロ回答中の不安全敵は攻撃意図の代わりに防御意図を表示する。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.UpdateIntent))]
[HarmonyPriority(450)]
public static class MarshmallowIntentDisplayPatch
{
    public static bool Prefix(NCreature __instance, IEnumerable<Creature> targets, ref Task __result)
    {
        if (SleepIntentPresentation.ShouldOverride(__instance.Entity))
            return true;

        if (!MarshmallowIntentPresentation.ShouldOverride(__instance.Entity))
            return true;

        __result = MarshmallowIntentPresentation.UpdateIntent(__instance, targets);
        return false;
    }
}
