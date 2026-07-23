using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>スパンキング — リプレイ付与を OnPlay より前（BeforeCardPlayed）で確定する。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
[HarmonyPriority(Priority.First)]
public static class SpankingReplayBeforePlayPatch
{
    public static void Prefix(ICombatState combatState, CardPlay play)
    {
        _ = combatState;
        if (play.Card is Spanking spanking)
            spanking.PrepareReplay(play.Target);
    }
}
