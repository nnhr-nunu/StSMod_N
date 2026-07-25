using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 説明文の [gold]トランス[/gold]／破滅／沼 に inline キーワードツールチップを付ける判定。
/// 性癖タグの <see cref="HcKeywords.Trance"/>（タイトル「トランス性癖」）とは別。
/// </summary>
public static class MechanicKeywordRules
{
    public static bool AppliesTranceKeyword(CardModel card) => HasDynamicVar(card, "Trance");

    public static bool AppliesDoomKeyword(CardModel card) => HasDynamicVar(card, "Doom");

    public static bool AppliesBogKeyword(CardModel card) => HasDynamicVar(card, "Bog");

    public static IEnumerable<CardKeyword> KeywordsFor(CardModel card)
    {
        if (AppliesTranceKeyword(card))
            yield return HcKeywords.TranceState;
        if (AppliesDoomKeyword(card))
            yield return HcKeywords.Doom;
        if (AppliesBogKeyword(card))
            yield return HcKeywords.Bog;
    }

    private static bool HasDynamicVar(CardModel card, string name) =>
        card.DynamicVars.Values.Any(v => v.Name == name);
}
