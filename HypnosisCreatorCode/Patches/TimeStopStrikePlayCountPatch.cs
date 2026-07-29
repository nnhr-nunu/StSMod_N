using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 時止めストライクのターン内プレイ回数。
/// GetResultLocationForCardPlay は OnPlay より先・複数回呼ばれるため加算は BeforeCardPlayed で1回だけ行う。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
public static class TimeStopStrikePlayCountPatch
{
    public static void Prefix(ICombatState combatState, CardPlay play)
    {
        _ = combatState;
        var card = play.Card.CanonicalInstance ?? play.Card;
        if (card is TimeStopStrike strike)
            strike.RecordPlayThisTurn();
    }
}
