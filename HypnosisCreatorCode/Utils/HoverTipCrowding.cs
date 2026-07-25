using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 拡大画面などでツールチップが縦に積みすぎるのを抑える。
/// スターターで学べる性癖3種と、説明文のトランス／破滅／沼キーワードを過多時だけ省略する。
/// </summary>
public static class HoverTipCrowding
{
    /// <summary>この件数以上（表示予定の合計）なら過多とみなす。</summary>
    public const int Threshold = 5;

    public static bool IsStarterFetishKeyword(CardKeyword keyword) =>
        keyword == HcKeywords.Sm || keyword == HcKeywords.DomSub || keyword == HcKeywords.Abnormal;

    public static bool IsOmittableKeywordWhenCrowded(CardKeyword keyword) =>
        IsStarterFetishKeyword(keyword)
        || keyword == HcKeywords.TranceState
        || keyword == HcKeywords.Doom
        || keyword == HcKeywords.Bog;

    public static bool ShouldOmitStarterFetishKeywordTips(
        CardModel card,
        IReadOnlySet<CardKeyword>? keywords = null) =>
        IsCrowded(card, keywords);

    public static bool IsCrowded(CardModel card, IReadOnlySet<CardKeyword>? keywords = null) =>
        EstimateIfShowingAll(card, keywords) >= Threshold;

    private static int EstimateIfShowingAll(CardModel card, IReadOnlySet<CardKeyword>? keywords) =>
        EstimateCore(card, keywords, skipOmittableKeywords: false);

    private static int EstimateCore(
        CardModel card,
        IReadOnlySet<CardKeyword>? keywords,
        bool skipOmittableKeywords)
    {
        var count = 0;
        if (card is HypnosisCreatorCard hc)
            count += hc.CountCardHoverTipsForCrowding();

        if (card.Enchantment != null)
            count += card.Enchantment.HoverTips.Count();
        if (card.Affliction != null)
            count += card.Affliction.HoverTips.Count();
        if (card.GetEnchantedReplayCount() > 0)
            count++;
        if (card.OrbEvokeType != OrbEvokeType.None)
            count++;
        if (card.GainsBlock)
            count++;

        foreach (var keyword in BuildEffectiveKeywordSet(card, keywords))
        {
            if (skipOmittableKeywords && IsOmittableKeywordWhenCrowded(keyword))
                continue;
            count++;
            if (keyword == CardKeyword.Ethereal)
                count++;
        }

        return count;
    }

    private static HashSet<CardKeyword> BuildEffectiveKeywordSet(
        CardModel card,
        IReadOnlySet<CardKeyword>? keywords)
    {
        var set = new HashSet<CardKeyword>(keywords ?? card.Keywords);
        foreach (var kw in FetishCardText.KeywordsFor(card))
            set.Add(kw);
        foreach (var kw in MechanicKeywordRules.KeywordsFor(card))
            set.Add(kw);
        return set;
    }
}
