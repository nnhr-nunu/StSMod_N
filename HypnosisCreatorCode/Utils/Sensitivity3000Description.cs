using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BaseLib.Patches.Localization;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 「必ず性癖に刺さる」等を1文字レインボー表示（感度3000倍・性癖の覇者・心酔・囁きUG 等）。
/// 文言は <c>cards.json</c> の <c>HYPNOSISCREATOR-SENSITIVITY3000.rainbowPhrase</c> から読む。
/// </summary>
public static class Sensitivity3000Description
{
    private const string LocTable = "cards";
    private const string RainbowPhraseKey = "HYPNOSISCREATOR-SENSITIVITY3000.rainbowPhrase";

    private const int RainbowPaletteVersion = 2;

    private static readonly ConcurrentDictionary<string, string> RainbowCache = new(StringComparer.Ordinal);

    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += Apply;

    private static void Apply(CardModel card, Creature? target, ref string description)
    {
        _ = target;
        _ = card;
        if (CardLibraryUiGuard.IsActive) return;

        var phrase = ResolveRainbowPhrase();
        if (string.IsNullOrEmpty(phrase)) return;
        // 文中では小文字始まりになる言語がある（eng "This card always hits Fetish." 等）ので大文字小文字は無視し、一致した綴りのまま虹色にする
        if (!description.Contains(phrase, StringComparison.OrdinalIgnoreCase)) return;

        description = Regex.Replace(
            description,
            Regex.Escape(phrase),
            static match => RainbowCache.GetOrAdd(
                $"{RainbowPaletteVersion}:{match.Value}",
                static key => RainbowText.LetterByLetter(key[(key.IndexOf(':') + 1)..])),
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string? ResolveRainbowPhrase()
    {
        try
        {
            var text = new LocString(LocTable, RainbowPhraseKey).GetFormattedText()?.Trim();
            if (IsResolvablePhrase(text)) return text;
        }
        catch
        {
            // ignore
        }

        // .pck 未更新などのフォールバック
        return UpgradeCardText.IsJapaneseUi() ? "必ず性癖に刺さる" : "Always hits Fetish";
    }

    private static bool IsResolvablePhrase(string? text) =>
        !string.IsNullOrWhiteSpace(text)
        && !text.StartsWith("HYPNOSISCREATOR-", StringComparison.Ordinal)
        && !text.Contains("rainbowPhrase", StringComparison.Ordinal);
}
