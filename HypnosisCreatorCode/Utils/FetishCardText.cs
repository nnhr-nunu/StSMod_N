using BaseLib.Patches.Localization;
using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// カード説明の最上部に性癖タグ（色付き・ツールチップ対応）を出す。
/// </summary>
public static class FetishCardText
{
    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += PrependFetishLine;

    public static CardKeyword? ToKeyword(FetishType type) => type switch
    {
        FetishType.Sm => HcKeywords.Sm,
        FetishType.DomSub => HcKeywords.DomSub,
        FetishType.Abnormal => HcKeywords.Abnormal,
        FetishType.Trance => HcKeywords.Trance,
        _ => null
    };

    public static IEnumerable<CardKeyword> KeywordsFor(CardModel card)
    {
        foreach (var fetish in CardFetishLookup.GetFetishes(card).Distinct())
        {
            var kw = ToKeyword(fetish);
            if (kw is { } keyword) yield return keyword;
        }
    }

    /// <summary>例: [gold]アブノーマル[/gold]/[gold]DomSub[/gold]。</summary>
    public static string? FormatPrefix(CardModel card)
    {
        var titles = new List<string>();
        foreach (var fetish in CardFetishLookup.GetFetishes(card).Distinct())
        {
            if (ToKeyword(fetish) is null) continue;
            titles.Add($"[gold]{LocalizedTitle(fetish)}[/gold]");
        }

        if (titles.Count == 0) return null;
        var period = IsJapaneseUi() ? "。" : ".";
        return string.Join("/", titles) + period;
    }

    private static string LocalizedTitle(FetishType type)
    {
        var jpn = IsJapaneseUi();
        return type switch
        {
            FetishType.Sm => "SM",
            FetishType.DomSub => "DomSub",
            FetishType.Abnormal => jpn ? "アブノーマル" : "Abnormal",
            FetishType.Trance => jpn ? "トランス" : "Trance",
            _ => type.ToString()
        };
    }

    private static bool IsJapaneseUi()
    {
        try
        {
            var lang = LocManager.Instance?.Language ?? "";
            return lang.Contains("jpn", StringComparison.OrdinalIgnoreCase)
                   || lang.Contains("ja", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static void PrependFetishLine(CardModel card, Creature? target, ref string description)
    {
        var prefix = FormatPrefix(card);
        if (prefix == null) return;
        if (description.StartsWith(prefix, StringComparison.Ordinal)) return;
        description = prefix + "\n" + description;
    }
}
