using System.Text.RegularExpressions;
using HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// UG説明の差分表示。説明全文の差し替えは禁止（[gold]キーワード／性癖プレフィックスが消える）。
/// 追加・置換箇所だけ [green] で示す。
/// </summary>
public static class UpgradeCardText
{
    public static string Green(string text) => $"[green]{text}[/green]";

    public static bool IsJapaneseUi()
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

    /// <summary>UG時のみ、説明末尾に緑の1行を追加する。</summary>
    public static void AppendGreenLine(
        CardModel card,
        ref string description,
        Func<CardModel, bool> match,
        string jpnLine,
        string engLine)
    {
        if (!match(card) || !card.IsUpgraded) return;

        var plain = IsJapaneseUi() ? jpnLine : engLine;
        if (string.IsNullOrWhiteSpace(plain)) return;
        if (description.Contains(plain, StringComparison.OrdinalIgnoreCase)) return;

        var line = Green(plain.TrimEnd());
        if (description.Contains(line, StringComparison.OrdinalIgnoreCase)) return;
        description = description.TrimEnd() + "\n" + line;
    }

    /// <summary>
    /// 暗示解除+: エナジー獲得2倍。CustomizeDescriptionPost は energyIcons 展開後のため、
    /// プレースホルダ文字列ではなく前後の固定文言で差し替える。
    /// </summary>
    public static void ApplySuggestionReleaseEnergyUpgrade(CardModel card, ref string description)
    {
        if (card is not SuggestionRelease || !card.IsUpgraded) return;

        if (IsJapaneseUi())
        {
            const string upgraded = "その数値の[green]2[/green]倍の";
            if (description.Contains(upgraded, StringComparison.Ordinal)) return;
            if (description.Contains("その数値に応じて", StringComparison.Ordinal))
            {
                description = description.Replace(
                    "その数値に応じて",
                    upgraded,
                    StringComparison.Ordinal);
            }

            return;
        }

        if (description.Contains("[green]twice[/green]", StringComparison.OrdinalIgnoreCase)) return;

        var match = Regex.Match(
            description,
            @"Gain (.+?) equal to the amount removed and",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success) return;

        var replacement =
            $"Gain [green]twice[/green] as much {match.Groups[1].Value} as the amount removed and";
        description = description[..match.Index]
                      + replacement
                      + description[(match.Index + match.Length)..];
    }

    /// <summary>UG時のみ、説明中の文言を置換する（to 側に [green] を含めてよい）。</summary>
    public static void ReplaceWhenUpgraded(
        CardModel card,
        ref string description,
        Func<CardModel, bool> match,
        string jpnFrom,
        string jpnTo,
        string engFrom,
        string engTo)
    {
        if (!match(card) || !card.IsUpgraded) return;

        if (IsJapaneseUi())
        {
            if (description.Contains(jpnTo, StringComparison.Ordinal)) return;
            if (description.Contains(jpnFrom, StringComparison.Ordinal))
                description = description.Replace(jpnFrom, jpnTo, StringComparison.Ordinal);
            return;
        }

        if (description.Contains(engTo, StringComparison.OrdinalIgnoreCase)) return;
        if (description.Contains(engFrom, StringComparison.OrdinalIgnoreCase))
            description = description.Replace(engFrom, engTo, StringComparison.OrdinalIgnoreCase);
    }

    // CustomizeDescriptionPost 用の薄いラッパ（Creature 引数を捨てる）
    public static void AppendGreenLine(
        CardModel card,
        Creature? _,
        ref string description,
        Func<CardModel, bool> match,
        string jpnLine,
        string engLine) =>
        AppendGreenLine(card, ref description, match, jpnLine, engLine);

    public static void ReplaceWhenUpgraded(
        CardModel card,
        Creature? _,
        ref string description,
        Func<CardModel, bool> match,
        string jpnFrom,
        string jpnTo,
        string engFrom,
        string engTo) =>
        ReplaceWhenUpgraded(card, ref description, match, jpnFrom, jpnTo, engFrom, engTo);
}
