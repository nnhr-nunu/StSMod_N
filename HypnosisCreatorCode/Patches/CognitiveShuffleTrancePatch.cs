using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 追跡対象のトランス解除・敵死亡時に認知シャッフルを終了予約する。
/// </summary>
[HarmonyPatch(typeof(TrancePower), nameof(PowerModel.AfterRemoved))]
public static class CognitiveShuffleTranceRemovedPatch
{
    public static void Postfix(TrancePower __instance) =>
        CognitiveShufflePower.NotifyTranceTargetChanged(__instance.Owner);
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDeath))]
public static class CognitiveShuffleDeathPatch
{
    public static void Postfix(ref Task __result, Creature creature)
    {
        if (creature is not { IsEnemy: true }) return;
        CognitiveShufflePower.NotifyTranceTargetChanged(creature);

        if (!CognitiveShuffleExpire.HasPending) return;

        var original = __result;
        __result = FinishDeathAsync(original);
    }

    private static async Task FinishDeathAsync(Task? original)
    {
        if (original != null)
            await original;
        await CognitiveShuffleExpire.RunPendingAsync();
    }
}
