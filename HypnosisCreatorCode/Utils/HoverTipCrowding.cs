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
    /// <summary>この件数以上（省略対象を除いた推定）なら過多とみなす。</summary>
    public const int Threshold = 5;

    public static bool IsStarterFetishKeyword(CardKeyword keyword) =>
        keyword == HcKeywords.Sm || keyword == HcKeywords.DomSub || keyword == HcKeywords.Abnormal;

    public static bool ShouldOmitStarterFetishKeywordTips(
        CardModel card,
        IReadOnlySet<CardKeyword>? keywords = null) =>
        IsCrowded(card, keywords);

    public static bool IsCrowded(CardModel card, IReadOnlySet<CardKeyword>? keywords = null) =>
        Estimate(card, includeStarterFetishKeywords: false, includeMechanicKeywords: false, keywords) >= Threshold;

    private static int Estimate(
        CardModel card,
        bool includeStarterFetishKeywords,
        bool includeMechanicKeywords,
        IReadOnlySet<CardKeyword>? keywords)
    {
        var count = 0;
        if (card is HypnosisCreatorCard hc)
        {
            count += hc.CountCardHoverTipsForCrowding();
            if (includeMechanicKeywords)
                count += MechanicKeywordRules.KeywordsFor(card).Count();
        }

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

        foreach (var keyword in keywords ?? card.Keywords)
        {
            if (!includeStarterFetishKeywords && IsStarterFetishKeyword(keyword))
                continue;
            count++;
            if (keyword == CardKeyword.Ethereal)
                count++;
        }

        return count;
    }
}
