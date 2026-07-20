using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 心停止が残り0（行動終了待ち）のとき、Amount=0 でもパワーを消さない。
/// </summary>
[HarmonyPatch(typeof(PowerModel), nameof(PowerModel.ShouldRemoveDueToAmount))]
public static class CardiacArrestKeepAtZeroPatch
{
    public static void Postfix(PowerModel __instance, ref bool __result)
    {
        if (__instance is CardiacArrestPower { KillAfterActionEnd: true })
            __result = false;
    }
}
