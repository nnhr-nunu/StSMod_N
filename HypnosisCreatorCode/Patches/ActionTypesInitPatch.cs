using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Modding;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// ModInitializer 内では ModManager 未完了のため ActionTypes が失敗する。
/// 全 mod 読込完了後に INetAction（心臓発動など）を登録する。
/// Initialize の戻り Task は差し替えない（差し替えると本家の完了待ちが壊れ、
/// マルチの mod リスト広告が欠けることがある）。
/// </summary>
[HarmonyPatch(typeof(ModManager), nameof(ModManager.Initialize))]
public static class ActionTypesInitPatch
{
    public static void Postfix(Task __result)
    {
        if (__result == null)
        {
            TryInitializeActionTypes();
            return;
        }

        _ = ContinueAsync(__result);
    }

    private static async Task ContinueAsync(Task original)
    {
        try
        {
            await original.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"ActionTypes init skipped; ModManager.Initialize failed: {ex.Message}");
            return;
        }

        TryInitializeActionTypes();
    }

    private static void TryInitializeActionTypes()
    {
        try
        {
            ActionTypes.Initialize();
            MainFile.Logger.Info("ActionTypes initialized after ModManager.");
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"ActionTypes.Initialize failed after ModManager: {ex.Message}");
        }
    }
}
