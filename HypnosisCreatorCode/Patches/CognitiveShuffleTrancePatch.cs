using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 追跡対象の敵死亡時に認知シャッフル終了を予約する（実行は <see cref="CognitiveShufflePower.AfterCardPlayed"/>）。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDeath))]
public static class CognitiveShuffleDeathPatch
{
    public static void Postfix(Creature creature)
    {
        if (creature is not { IsEnemy: true }) return;
        CognitiveShufflePower.NotifyTranceTargetChanged(creature);
    }
}
