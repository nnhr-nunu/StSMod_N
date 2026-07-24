using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 認知シャッフル — カードプレイ完了後にパワー付与（OnPlay 内 PowerCmd は宙吊りの原因）。
/// 毎回 Task を延長し、選択予約があるときだけ付与する（早期 return だと __result 連鎖が抜ける）。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class CognitiveShuffleDeferredPatch
{
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(
        ref Task __result,
        ICombatState combatState,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        _ = combatState;
        var original = __result;
        __result = FinishCardPlayAsync(original, choiceContext, cardPlay);
    }

    private static async Task FinishCardPlayAsync(
        Task? original,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (original != null)
            await original;

        await CognitiveShuffleCompletion.RunIfPendingAsync(choiceContext, cardPlay);
    }
}
