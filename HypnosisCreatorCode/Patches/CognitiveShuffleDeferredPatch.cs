using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 認知シャッフル — カードプレイ完了後にパワー付与・期限切れ（OnPlay / 同期フック内 PowerCmd は宙吊りの原因）。
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
        var owner = cardPlay.Card.Owner;
        var hasPending = owner is Player player && CognitiveShufflePendingStore.HasPending(player);

        if (!hasPending && !CognitiveShuffleExpire.HasPending)
            return;

        var original = __result;
        __result = FinishCardPlayAsync(original, choiceContext, cardPlay, hasPending);
    }

    private static async Task FinishCardPlayAsync(
        Task? original,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        bool runCompletion)
    {
        if (original != null)
            await original;

        if (runCompletion)
            await CognitiveShuffleCompletion.RunIfPendingAsync(choiceContext, cardPlay);

        await CognitiveShuffleExpire.RunPendingAsync();
    }
}
