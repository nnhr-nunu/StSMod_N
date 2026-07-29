using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 時止めストライクのターン内プレイ回数。GetResultLocationForCardPlay の複数呼び出しで
/// 加算されないよう、プレイ確定後に1回だけ記録する。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class TimeStopStrikePlayCountPatch
{
    public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay play)
    {
        _ = combatState;
        _ = choiceContext;
        if (play.Card is TimeStopStrike strike)
            strike.RecordPlayThisTurn();
    }
}
