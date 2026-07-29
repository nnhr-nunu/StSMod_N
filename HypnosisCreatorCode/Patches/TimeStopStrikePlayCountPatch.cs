using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 時止めストライクのプレイ回数フラグを OnPlayWrapper 終了時に戻す。
/// 加算は <see cref="TimeStopStrike.GetResultLocationForCardPlay"/>（OnPlayWrapper 内・BeforeCardPlayed より先）で行う。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
public static class TimeStopStrikePlayCountPatch
{
    public static void Postfix(CardModel __instance)
    {
        if (__instance is TimeStopStrike strike)
            strike.FinishPlayWrapper();
    }
}
