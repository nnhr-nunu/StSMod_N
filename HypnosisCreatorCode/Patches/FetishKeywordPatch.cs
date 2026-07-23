using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 性癖タグに応じて Keywords へ足し、説明中の色付き文言にツールチップを付ける。
/// HCカードおよび他色アブノーマル対象カードに適用。
/// ツールチップ過多時はスターターで学べる SM／DomSub／アブノーマルを省略（<see cref="HoverTipCrowding"/>）。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetKeywordsWithSources))]
public static class FetishKeywordPatch
{
    public static void Postfix(
        CardModel __instance,
        KeywordSources sources,
        ref IReadOnlySet<CardKeyword> __result)
    {
        var extra = FetishCardText.KeywordsFor(__instance).ToList();
        if (extra.Count == 0) return;

        if (HoverTipCrowding.ShouldOmitStarterFetishKeywordTips(__instance, __result))
            extra.RemoveAll(HoverTipCrowding.IsStarterFetishKeyword);
        if (extra.Count == 0) return;

        if (extra.All(__result.Contains)) return;

        var merged = new HashSet<CardKeyword>(__result);
        foreach (var kw in extra)
            merged.Add(kw);
        __result = merged;
    }
}
