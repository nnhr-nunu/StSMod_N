using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 認知シャッフル — カードプレイ演出完了後にパワー付与（OnPlay 内 PowerCmd は宙吊りの原因）。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class CognitiveShuffleDeferredPatch
{
    /// <summary>他の AfterCardPlayed 処理のあとで付与し、Task を延長して次カードと競合しない。</summary>
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(
        ref Task __result,
        ICombatState combatState,
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        _ = combatState;
        var owner = cardPlay.Card.Owner;
        if (owner is not Player player || !CognitiveShufflePendingStore.HasPending(player))
            return;

        var original = __result;
        __result = CognitiveShuffleCompletion.CompleteAfterPlayedAsync(original, choiceContext, cardPlay);
    }
}
