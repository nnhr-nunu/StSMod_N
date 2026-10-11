using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Localization;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// C# 側で組み立てる表示文（戦闘プレビュー括弧・性癖表示など）を全言語のローカライズから引く。
/// 文面は <c>gameplay_ui.json</c>、キーワード名は <c>card_keywords.json</c> が正本。
/// キーが引けないとき（.pck 未更新など）だけ日本語／英語のフォールバックを使う。
/// </summary>
public static class ModLocText
{
    private const string GameplayUiTable = "gameplay_ui";
    private const string CardKeywordsTable = "card_keywords";
    private const string Sentinel = "\u0001";

    private static readonly ConcurrentDictionary<string, Regex?> PatternCache = new(StringComparer.Ordinal);

    /// <summary><c>gameplay_ui</c> の文面。<paramref name="addVariables"/> で <c>{Name}</c> を埋める。</summary>
    public static string Get(
        string key,
        string japaneseFallback,
        string englishFallback,
        Action<LocString>? addVariables = null) =>
        TryGet(key, addVariables) ?? (UpgradeCardText.IsJapaneseUi() ? japaneseFallback : englishFallback);

    /// <summary>改行区切りの複数行（吹き出し候補など）。引けなければ null。</summary>
    public static string[]? GetLines(string key)
    {
        var text = TryGet(key, null);
        if (text == null) return null;
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return lines.Length > 0 ? lines : null;
    }

    /// <summary>キーワード名（例: <c>RETAIN.title</c>、<c>HYPNOSISCREATOR-ABNORMAL.title</c>）。引けなければ null。</summary>
    public static string? KeywordTitle(string titleKey)
    {
        try
        {
            var text = LocString.GetIfExists(CardKeywordsTable, titleKey)?.GetFormattedText()?.Trim();
            return string.IsNullOrEmpty(text) || text.Contains(titleKey, StringComparison.Ordinal) ? null : text;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>本家の文末記号（jpn/zhs「。」、他「.」）。</summary>
    public static string Period()
    {
        try
        {
            var text = LocString.GetIfExists(CardKeywordsTable, "PERIOD")?.GetRawText();
            if (!string.IsNullOrEmpty(text)) return text;
        }
        catch
        {
            // フォールバックへ
        }

        return UpgradeCardText.IsJapaneseUi() ? "。" : ".";
    }

    /// <summary>本家がカード説明へ自動挿入するキーワード行（<c>[gold]保留[/gold]。</c> 等）。引けなければ null。</summary>
    public static string? KeywordCardText(string keywordKeyPrefix)
    {
        var title = KeywordTitle(keywordKeyPrefix + ".title");
        return title == null ? null : $"[gold]{title}[/gold]{Period()}";
    }

    /// <summary>
    /// 本文中の <c>[gold]キーワード名[/gold]</c> に一致する正規表現。言語ごとにキャッシュ。
    /// ロシア語などの語形変化（<c>Транс</c>→<c>Трансе</c>）は語末だけ許容する。漢字・ハングルは完全一致。
    /// </summary>
    public static Regex? KeywordMentionPattern(string titleKey)
    {
        var cacheKey = $"{CurrentLanguage()}:mention:{titleKey}";
        return PatternCache.GetOrAdd(cacheKey, _ =>
        {
            var title = KeywordTitle(titleKey);
            if (title == null) return null;

            var words = title.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Length >= 3 && w.Any(char.IsLower)
                    ? Regex.Escape(w[..^1]) + @"\p{L}{0,3}"
                    : Regex.Escape(w));
            var pattern = @"\[gold\]" + string.Join(@"\s+", words) + @"\[/gold\]";
            return new Regex(pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        });
    }

    /// <summary>行頭のキーワード行（<see cref="KeywordCardText"/>）と直後の改行に一致する正規表現。言語ごとにキャッシュ。</summary>
    public static Regex? KeywordLinePattern(string keywordKeyPrefix)
    {
        var cacheKey = $"{CurrentLanguage()}:keyword-line:{keywordKeyPrefix}";
        return PatternCache.GetOrAdd(cacheKey, _ =>
        {
            var text = KeywordCardText(keywordKeyPrefix);
            return text == null
                ? null
                : new Regex("^" + Regex.Escape(text) + @"(?:\r?\n)?", RegexOptions.CultureInvariant | RegexOptions.Multiline);
        });
    }

    /// <summary>
    /// <paramref name="key"/> の文面を、変数部分を任意文字列にした「末尾一致」の正規表現にする（括弧の付け直し用）。
    /// 言語ごとにキャッシュ。引けなければ null。
    /// </summary>
    public static Regex? SuffixPattern(string key, params string[] variableNames)
    {
        var cacheKey = $"{CurrentLanguage()}:{key}";
        return PatternCache.GetOrAdd(cacheKey, _ =>
        {
            var text = TryGet(key, loc =>
            {
                foreach (var name in variableNames)
                    loc.Add(name, Sentinel);
            });
            if (string.IsNullOrWhiteSpace(text)) return null;

            var pattern = Regex.Escape(text).Replace(Sentinel, ".*?", StringComparison.Ordinal) + @"\s*$";
            return new Regex(pattern, RegexOptions.CultureInvariant);
        });
    }

    private static string? TryGet(string key, Action<LocString>? addVariables)
    {
        try
        {
            var loc = LocString.GetIfExists(GameplayUiTable, key);
            if (loc == null) return null;

            addVariables?.Invoke(loc);
            var text = loc.GetFormattedText();
            if (string.IsNullOrWhiteSpace(text) || text.Contains(key, StringComparison.Ordinal))
                return null;
            return text;
        }
        catch
        {
            return null;
        }
    }

    private static string CurrentLanguage()
    {
        try
        {
            return LocManager.Instance?.Language ?? "";
        }
        catch
        {
            return "";
        }
    }
}
