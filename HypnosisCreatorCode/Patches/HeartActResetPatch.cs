using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 層（アクト）進入時に希少心臓の使用済みフラグをリセットする。
/// フックの完了後に行う（戻り値なしの Postfix は最初の待ちの時点で走り、早送り設定で位置が変わる）。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterActEntered))]
public static class HeartActResetPatch
{
    public static void Postfix(ref Task __result, IRunState runState) =>
        __result = RefreshAfterAsync(__result, runState);

    private static async Task RefreshAfterAsync(Task original, IRunState runState)
    {
        await original;
        foreach (var player in runState.Players)
            HeartInventory.RefreshAllForNewAct(player);
    }
}
