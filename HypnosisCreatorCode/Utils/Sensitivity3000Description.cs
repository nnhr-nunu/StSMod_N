using System.Collections.Concurrent;
using BaseLib.Patches.Localization;
using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 感度3000倍 —「必ず性癖に刺さる」等を1文字レインボー表示。
/// 文言は <c>cards.json</c> の <c>.rainbowPhrase</c> から読み、言語追加時はキーを足すだけ。
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
        if (card is not Sensitivity3000) return;

        var phrase = ResolveRainbowPhrase();
        if (string.IsNullOrEmpty(phrase)) return;
        if (!description.Contains(phrase, StringComparison.Ordinal)) return;

        var rainbow = RainbowCache.GetOrAdd(
            $"{RainbowPaletteVersion}:{phrase}",
            static key => RainbowText.LetterByLetter(key[(key.IndexOf(':') + 1)..]));
        description = description.Replace(phrase, rainbow, StringComparison.Ordinal);
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
