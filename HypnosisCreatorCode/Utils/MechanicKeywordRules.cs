using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 説明文の [gold]トランス[/gold]／破滅／沼 に inline キーワードツールチップを付ける判定。
/// 性癖タグの <see cref="HcKeywords.Trance"/>（タイトル「トランス性癖」）とは別。
/// </summary>
public static class MechanicKeywordRules
{
    private const string CardsLocTable = "cards";

    public static bool AppliesTranceKeyword(CardModel card) =>
        HasDynamicVar(card, "Trance")
        || MentionsGoldInCardDescription(card, "トランス")
        || MentionsGoldInCardDescription(card, "Trance");

    public static bool AppliesDoomKeyword(CardModel card) =>
        HasDynamicVar(card, "Doom")
        || MentionsGoldInCardDescription(card, "破滅")
        || MentionsGoldInCardDescription(card, "Doom");

    public static bool AppliesBogKeyword(CardModel card) =>
        HasDynamicVar(card, "Bog")
        || MentionsGoldInCardDescription(card, "沼")
        || MentionsGoldInCardDescription(card, "Bog");

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

    private static bool MentionsGoldInCardDescription(CardModel card, string goldText)
    {
        var text = GetCardDescriptionText(card);
        return text != null
            && text.Contains($"[gold]{goldText}[/gold]", StringComparison.Ordinal);
    }

    private static string? GetCardDescriptionText(CardModel card)
    {
        try
        {
            return new LocString(CardsLocTable, $"{card.Id.Entry}.description")
                .GetFormattedText();
        }
        catch
        {
            return null;
        }
    }
}
