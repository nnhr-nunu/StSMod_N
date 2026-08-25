using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// UG説明の差分表示。定性差分は <c>cards.json</c> の
/// <c>.upgradeAppend</c> / <c>.upgradeReplaceFrom</c> / <c>.upgradeReplaceTo</c> を正本とする。
/// </summary>
public static class UpgradeCardText
{
    private const string LocTable = "cards";
    private const string UpgradeAppendSuffix = ".upgradeAppend";
    private const string UpgradeReplaceFromSuffix = ".upgradeReplaceFrom";
    private const string UpgradeReplaceToSuffix = ".upgradeReplaceTo";
    private const string UpgradeEnergyMultiplierSuffix = ".upgradeEnergyMultiplier";

    public static string Green(string text) => $"[green]{text}[/green]";

    /// <summary>UG説明差分を loc キーから適用する（カードライブラリ表示でも有効）。</summary>
    public static void ApplyLocalizedUpgrade(CardModel card, ref string description)
    {
        if (!card.IsUpgraded) return;

        TryApplyReplace(card, ref description);
        TryApplyAppend(card, ref description);
    }

    private static void TryApplyReplace(CardModel card, ref string description)
    {
        if (!TryGetFormattedUpgradeLocText(card, UpgradeReplaceFromSuffix, out var from))
            return;

        TryGetFormattedUpgradeLocText(card, UpgradeReplaceToSuffix, out var to);

        if (!string.IsNullOrEmpty(to) && description.Contains(to, StringComparison.Ordinal))
            return;

        if (description.Contains(from, StringComparison.Ordinal))
        {
            description = description.Replace(from, to, StringComparison.Ordinal);
            return;
        }

        if (card.Id.Entry.EndsWith("SUGGESTION_RELEASE", StringComparison.Ordinal))
            TryApplySuggestionReleaseEnergyFallback(card, ref description, from, to);
    }

    private static void TryApplyAppend(CardModel card, ref string description)
    {
        if (!TryGetCardLocText(card, UpgradeAppendSuffix, out var plain))
            return;

        plain = plain.TrimEnd();
        if (string.IsNullOrWhiteSpace(plain)) return;
        if (description.Contains(plain, StringComparison.OrdinalIgnoreCase)) return;

        var line = Green(plain);
        if (description.Contains(line, StringComparison.OrdinalIgnoreCase)) return;
        description = description.TrimEnd() + "\n" + line;
    }

    /// <summary>
    /// 暗示解除+: 英語など energyIcons 展開後は単純置換が効かない場合のフォールバック。
    /// </summary>
    private static void TryApplySuggestionReleaseEnergyFallback(
        CardModel card,
        ref string description,
        string from,
        string to)
    {
        if (!card.Id.Entry.EndsWith("SUGGESTION_RELEASE", StringComparison.Ordinal))
            return;

        if (!string.IsNullOrEmpty(from) && description.Contains(from, StringComparison.Ordinal))
        {
            description = description.Replace(from, to, StringComparison.Ordinal);
            return;
        }

        if (!TryGetCardLocText(card, UpgradeEnergyMultiplierSuffix, out var multiplier)
            || string.IsNullOrWhiteSpace(multiplier))
        {
            return;
        }

        if (description.Contains($"[green]{multiplier}[/green]", StringComparison.OrdinalIgnoreCase))
            return;

        var match = Regex.Match(
            description,
            @"Gain (.+?) equal to the amount removed and",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!match.Success) return;

        var replacement =
            $"Gain [green]{multiplier}[/green] as much {match.Groups[1].Value} as the amount removed and";
        description = description[..match.Index]
                      + replacement
                      + description[(match.Index + match.Length)..];
    }

    private static bool TryGetCardLocText(CardModel card, string suffix, out string text)
    {
        text = "";
        var key = card.Id.Entry + suffix;
        try
        {
            text = new LocString(LocTable, key).GetFormattedText() ?? "";
        }
        catch
        {
            return false;
        }

        if (IsUnresolvedLoc(key, text))
        {
            text = "";
            return false;
        }

        return true;
    }

    /// <summary>
    /// upgradeReplace* は説明と同じ DynamicVar / IfUpgraded 文脈で展開する（{Damage:diff()} 等）。
    /// </summary>
    private static bool TryGetFormattedUpgradeLocText(CardModel card, string suffix, out string text)
    {
        text = "";
        var key = card.Id.Entry + suffix;
        try
        {
            var loc = new LocString(LocTable, key);
            card.DynamicVars.AddTo(loc);
            var upgradeDisplay = card.IsUpgraded ? UpgradeDisplay.Upgraded : UpgradeDisplay.Normal;
            loc.Add(new IfUpgradedVar(upgradeDisplay));
            text = loc.GetFormattedText() ?? "";
        }
        catch
        {
            return false;
        }

        if (IsUnresolvedLoc(key, text))
        {
            text = "";
            return false;
        }

        return true;
    }

    private static bool IsUnresolvedLoc(string key, string text) =>
        string.IsNullOrWhiteSpace(text)
        || string.Equals(text, key, StringComparison.Ordinal)
        || text.Contains(key, StringComparison.Ordinal);

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

}
