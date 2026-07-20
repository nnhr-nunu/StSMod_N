using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 本家「締め付け」は文言が絞蛇固定。ヒプノクリエイターが付与したときだけキャラ名に差し替える。
/// 絞蛇などが付与したインスタンスは本家文言のまま（ソロ／マルチ共通）。
/// </summary>
[HarmonyPatch(typeof(PowerModel), "get_Description")]
public static class ConstrictPowerHcDescriptionPatch
{
    public static void Postfix(PowerModel __instance, ref LocString __result)
    {
        if (!ConstrictPowerHcText.ShouldUseHcText(__instance)) return;
        __result = ConstrictPowerHcText.Description;
    }
}

[HarmonyPatch(typeof(PowerModel), "get_SmartDescription")]
public static class ConstrictPowerHcSmartDescriptionPatch
{
    public static void Postfix(PowerModel __instance, ref LocString __result)
    {
        if (!ConstrictPowerHcText.ShouldUseHcText(__instance)) return;
        __result = ConstrictPowerHcText.SmartDescription;
    }
}
