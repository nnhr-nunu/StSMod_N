using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 認知シャッフルで得た虚無化は、カードプレイ後の強制ターン終了をスキップする。
/// </summary>
[HarmonyPatch(typeof(VoidFormPower), nameof(VoidFormPower.AfterCardPlayed))]
public static class VoidFormNoEndTurnPatch
{
    public static bool Prefix(VoidFormPower __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (__instance.Owner?.GetPower<CognitiveVoidBypassPower>() != null)
            return false;
        return true;
    }
}
