using System.Collections.Concurrent;
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

    private static readonly ConcurrentDictionary<string, int> EstimateCache = new(StringComparer.Ordinal);

    public static bool IsStarterFetishKeyword(CardKeyword keyword) =>
        keyword == HcKeywords.Sm || keyword == HcKeywords.DomSub || keyword == HcKeywords.Abnormal;

    public static bool IsOmittableKeywordWhenCrowded(CardKeyword keyword) =>
        IsStarterFetishKeyword(keyword)
        || keyword == HcKeywords.TranceState
        || keyword == HcKeywords.Doom
        || keyword == HcKeywords.Bog;

    public static bool ShouldOmitStarterFetishKeywordTips(CardModel card) =>
        IsCrowded(card);

    /// <summary>過多時は <see cref="HypnosisCreatorCard.CardHoverTips"/> も出さない（戦闘ホバーの縦はみ出し防止）。</summary>
    public static bool ShouldOmitCardHoverTips(CardModel card) =>
        IsCrowded(card);

    public static bool IsCrowded(CardModel card) =>
        EstimateIfShowingAll(card) >= Threshold;

    private static int EstimateIfShowingAll(CardModel card)
    {
        if (KeywordPatchGuard.IsNested)
            return Threshold;

        if (HasInstanceSpecificTips(card))
            return EstimateCore(card, skipOmittableKeywords: false);

        var entry = card.Id?.Entry;
        if (string.IsNullOrEmpty(entry))
            return EstimateCore(card, skipOmittableKeywords: false);

        if (EstimateCache.TryGetValue(entry, out var cached))
            return cached;

        var estimate = EstimateCore(card, skipOmittableKeywords: false);
        EstimateCache.TryAdd(entry, estimate);
        return estimate;
    }

    private static bool HasInstanceSpecificTips(CardModel card) =>
        card.Enchantment != null
        || card.Affliction != null
        || card.GetEnchantedReplayCount() > 0
        || card.OrbEvokeType != OrbEvokeType.None
        || card.GainsBlock;

    private static int EstimateCore(
        CardModel card,
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

        foreach (var keyword in BuildEffectiveKeywordSet(card))
        {
            if (skipOmittableKeywords && IsOmittableKeywordWhenCrowded(keyword))
                continue;
            count++;
            if (keyword == CardKeyword.Ethereal)
                count++;
        }

        return count;
    }

    private static HashSet<CardKeyword> BuildEffectiveKeywordSet(CardModel card)
    {
        var set = card.Keywords is null
            ? new HashSet<CardKeyword>()
            : new HashSet<CardKeyword>(card.Keywords);

        // 正規テンプレ（虫殺しイベントのホバー等）では mod キーワードを足さない。
        if (!card.IsCanonical)
        {
            foreach (var kw in FetishCardText.KeywordsFor(card))
                set.Add(kw);
            foreach (var kw in MechanicKeywordRules.KeywordsFor(card))
                set.Add(kw);
        }

        return set;
    }
}
