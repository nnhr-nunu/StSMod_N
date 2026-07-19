using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// CardFetishes に応じて性癖キーワードを Keywords へ足し、説明中の色付き文言にツールチップを付ける。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetKeywordsWithSources))]
public static class FetishKeywordPatch
{
    public static void Postfix(
        CardModel __instance,
        KeywordSources sources,
        ref IReadOnlySet<CardKeyword> __result)
    {
        if (__instance is not HypnosisCreatorCard hc) return;
        if (hc.CardFetishes.Count == 0) return;

        var extra = FetishCardText.KeywordsFor(hc).ToList();
        if (extra.Count == 0) return;
        if (extra.All(__result.Contains)) return;

        var merged = new HashSet<CardKeyword>(__result);
        foreach (var kw in extra)
            merged.Add(kw);
        __result = merged;
    }
}
