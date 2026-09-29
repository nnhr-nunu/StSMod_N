using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 時止めストライク／ティンシャのプレイ回数フラグを OnPlayWrapper 終了時に戻す。
/// 加算は <see cref="TimeStopStrike.GetResultLocationForCardPlay"/>（OnPlayWrapper 内・BeforeCardPlayed より先）で行う。
/// 戻り値なしの Postfix だと OnPlayWrapper の最初の待ち（早送り設定や自動プレイで有無が変わる）で戻ってしまい、
/// 加算の前後が端末ごとにずれて行き先（手札／捨て札）が変わる。完了後（例外時も）に戻す。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
public static class TimeStopStrikePlayCountPatch
{
    public static void Postfix(CardModel __instance, ref Task __result)
    {
        if (__instance is TimeStopStrike or Tingsha)
            __result = FinishAfterAsync(__result, __instance);
    }

    private static async Task FinishAfterAsync(Task original, CardModel card)
    {
        try
        {
            await original;
        }
        finally
        {
            switch (card)
            {
                case TimeStopStrike strike:
                    strike.FinishPlayWrapper();
                    break;
                case Tingsha tingsha:
                    tingsha.FinishPlayWrapper();
                    break;
            }
        }
    }
}
