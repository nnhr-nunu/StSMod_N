using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 植物寄生など「戦闘終了時に心臓」予約を、追加レリック報酬として解決する。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatEnd))]
public static class HeartCaptureCombatEndPatch
{
    public static void Postfix(ref Task __result)
    {
        var original = __result;
        __result = ContinueAfterCombatEnd(original);
    }

    private static async Task ContinueAfterCombatEnd(Task original)
    {
        if (original != null)
            await original;

        HeartCapture.FlushPending();
    }
}
