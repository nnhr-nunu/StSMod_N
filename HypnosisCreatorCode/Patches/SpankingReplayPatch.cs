using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// スパンキング／糸色丁頁／性癖の覇者／連続トランス — リプレイ付与は OnPlayWrapper 内の GeneratePlayCount より前に確定させる。
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
        else if (__instance is InfiniteUpgradeString infiniteUpgradeString)
            infiniteUpgradeString.PrepareReplay();
        else if (__instance is FetishChampion fetishChampion)
            fetishChampion.PrepareReplay(target);
        else if (__instance is ContinuousTrance continuousTrance)
            continuousTrance.PrepareReplay();
    }
}

/// <summary>
/// 連続トランス — 正本（immutable）カードは BaseReplayCount を書けないが、説明のリプレイ行は GetEnchantedReplayCount から組み立てられる。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetEnchantedReplayCount))]
public static class ContinuousTranceReplayDisplayPatch
{
    public static void Postfix(CardModel __instance, ref int __result)
    {
        if (__instance is ContinuousTrance)
            __result = Math.Max(__result, 1);
    }
}
