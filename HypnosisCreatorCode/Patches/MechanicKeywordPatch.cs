using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// Trance／破滅／沼の DynamicVar を持つカードに、説明文の金文字と一致するキーワードを付与する。
/// ツールチップ過多時は <see cref="HoverTipCrowding"/> と同条件で省略する。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetKeywordsWithSources))]
public static class MechanicKeywordPatch
{
    public static void Postfix(
        CardModel __instance,
        ref IReadOnlySet<CardKeyword> __result)
    {
        if (HoverTipCrowding.IsCrowded(__instance, __result))
            return;

        var extra = MechanicKeywordRules.KeywordsFor(__instance).ToList();
        if (extra.Count == 0) return;
        if (extra.All(__result.Contains)) return;

        var merged = new HashSet<CardKeyword>(__result);
        foreach (var kw in extra)
            merged.Add(kw);
        __result = merged;
    }
}
