using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// スパンキング — リプレイ付与は OnPlayWrapper 内の GeneratePlayCount より前に確定させる。
/// BeforeCardPlayed では遅い（PlayCount は既に確定済み）。
/// </summary>
[HarmonyPatch(typeof(CardModel), "GeneratePlayCount")]
[HarmonyPriority(Priority.First)]
public static class SpankingReplayGeneratePlayCountPatch
{
    public static void Prefix(CardModel __instance, ICombatState combatState, Creature target)
    {
        _ = combatState;
        if (__instance is Spanking spanking)
            spanking.PrepareReplay(target);
    }
}
