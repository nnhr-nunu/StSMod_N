using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 不安全敵のスライム催眠中は攻撃意図の代わりに粘液付与アイコンを表示する（NextMove は変更しない）。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.UpdateIntent))]
[HarmonyPriority(Priority.Last)]
public static class SlimeIntentDisplayPatch
{
    public static bool Prefix(NCreature __instance, IEnumerable<Creature> targets, ref Task __result)
    {
        if (SleepIntentPresentation.ShouldOverride(__instance.Entity))
            return true;

        if (!SlimeIntentPresentation.ShouldOverride(__instance.Entity))
            return true;

        __result = SlimeIntentPresentation.UpdateIntent(__instance, targets);
        return false;
    }
}
