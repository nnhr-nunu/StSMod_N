using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 追跡対象の敵死亡時に認知シャッフル終了を予約する（実行は <see cref="CognitiveShufflePower.AfterCardPlayed"/>）。
/// 死亡フックの完了後に行う（戻り値なしの Postfix は最初の待ちの時点で走り、早送り設定で位置が変わる）。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDeath))]
public static class CognitiveShuffleDeathPatch
{
    public static void Postfix(ref Task __result, Creature creature)
    {
        if (creature is not { IsEnemy: true }) return;
        __result = NotifyAfterAsync(__result, creature);
    }

    private static async Task NotifyAfterAsync(Task original, Creature creature)
    {
        await original;
        CognitiveShufflePower.NotifyTranceTargetChanged(creature);
    }
}
