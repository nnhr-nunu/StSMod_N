using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 認知シャッフル付与完了前に次カードを打つと PowerCmd が競合して宙吊りするため待機する。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
public static class CognitiveShuffleBeforeCardPatch
{
    [HarmonyPriority(Priority.First)]
    public static void Prefix(ref Task __result, ICombatState combatState, CardPlay cardPlay)
    {
        _ = combatState;
        if (cardPlay.Card is CognitiveShuffle) return;

        var wait = CognitiveShuffleCompletion.WaitForGateAsync(cardPlay.Card.Owner);
        if (wait.IsCompletedSuccessfully) return;

        var original = __result;
        __result = ContinueAfterGateAsync(original, wait);
    }

    private static async Task ContinueAfterGateAsync(Task? original, Task wait)
    {
        await wait;
        if (original != null)
            await original;
    }
}
