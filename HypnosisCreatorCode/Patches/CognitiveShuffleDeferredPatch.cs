using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 認知シャッフル — カードプレイ演出完了後にパワー付与（OnPlay 内 PowerCmd は宙吊りの原因）。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class CognitiveShuffleDeferredPatch
{
    public static void Postfix(PlayerChoiceContext choiceContext, CardPlay play) =>
        CognitiveShuffleCompletion.TrySchedule(choiceContext, play);
}
