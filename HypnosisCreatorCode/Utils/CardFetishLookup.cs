using HypnosisCreator.HypnosisCreatorCode.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>HCカードと他色アブノーマル付与カードの性癖タグを統一取得する。</summary>
public static class CardFetishLookup
{
    public static IReadOnlyList<FetishType> GetFetishes(CardModel card)
    {
        if (card is HypnosisCreatorCard hc)
            return hc.CardFetishes;

        if (AbnormalOtherColorPool.Contains(card))
            return [FetishType.Abnormal];

        return [];
    }

    public static bool AlwaysHitsFetish(CardModel card) =>
        card is HypnosisCreatorCard { AlwaysHitsFetish: true };

    public static bool HasAnyFetish(CardModel card) =>
        GetFetishes(card).Count > 0 || AlwaysHitsFetish(card);

    /// <summary>CanonicalVars に Trance があり、付与時のトランス性癖刺さりが起きうるカード。</summary>
    public static bool AppliesTrance(CardModel card)
    {
        foreach (var v in card.DynamicVars.Values)
        {
            if (v.Name == "Trance" && v.BaseValue > 0M)
                return true;
        }

        return false;
    }
}
