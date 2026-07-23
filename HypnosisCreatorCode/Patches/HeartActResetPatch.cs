using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>層（アクト）進入時に希少心臓の使用済みフラグをリセットする。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterActEntered))]
public static class HeartActResetPatch
{
    public static void Postfix(IRunState runState)
    {
        foreach (var player in runState.Players)
            HeartInventory.RefreshAllForNewAct(player);
    }
}
