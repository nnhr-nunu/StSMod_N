using HarmonyLib;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Modding;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// ModInitializer 内では ModManager 未完了のため ActionTypes が失敗する。
/// 全 mod 読込完了後に INetAction（心臓発動など）を登録する。
/// </summary>
[HarmonyPatch(typeof(ModManager), nameof(ModManager.Initialize))]
public static class ActionTypesInitPatch
{
    public static void Postfix(ref Task __result)
    {
        var original = __result;
        __result = ContinueAsync(original);
    }

    private static async Task ContinueAsync(Task original)
    {
        await original;
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
