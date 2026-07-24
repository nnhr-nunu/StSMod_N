using System.Text.RegularExpressions;
using BaseLib.Patches.Localization;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 先頭・末尾で連続するキーワード専用行（性癖・カウント・廃棄・リプレイ等）を1行にまとめる。
/// トランス付与だけの短い行（{Trance:diff()} 等のみ）も先頭ヘッダーとして結合する。
/// </summary>
public static class KeywordLineLayout
{
    private static readonly Regex ColorTag = new(
        @"\[(?:gold|blue|green)\][^\[]+\[/(?:gold|blue|green)\]",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex DynamicPlaceholder = new(
        @"\{[^{}]+\}",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += CollapseSafely;

    private static void CollapseSafely(CardModel card, Creature? target, ref string description)
    {
        try
        {
            CollapseConsecutiveKeywordLines(card, target, ref description);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Keyword line layout failed for {card.Id}: {ex.Message}");
        }
    }

    private static void CollapseConsecutiveKeywordLines(CardModel card, Creature? target, ref string description)
    {
        _ = card;
        _ = target;
        if (string.IsNullOrWhiteSpace(description)) return;

        var lines = description.Split('\n').ToList();
        if (lines.Count <= 1) return;

        CollapseRunFromStart(lines);
        CollapseRunFromEnd(lines);

        description = string.Join('\n', lines);
    }

    private static bool IsKeywordOnlyLine(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0) return false;
        if (trimmed.Contains('{', StringComparison.Ordinal)) return false;

        var stripped = ColorTag.Replace(trimmed, "");
        stripped = stripped.Replace("/", "", StringComparison.Ordinal).Trim();
        return stripped is "。" or ".";
    }

    /// <summary>タグと数値プレースホルダだけの短い行（例: トランス3。／{Trance:diff()}。）。</summary>
    private static bool IsCompactOpenerLine(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0) return false;
        if (!ColorTag.IsMatch(trimmed) && !trimmed.Contains('{', StringComparison.Ordinal)) return false;

        var withoutTags = ColorTag.Replace(trimmed, "");
        withoutTags = withoutTags.Replace("/", "", StringComparison.Ordinal);
        withoutTags = DynamicPlaceholder.Replace(withoutTags, "");
        withoutTags = withoutTags.Trim();

        // タグ・プレースホルダ除去後は句点だけ（本文テキストなし）
        return withoutTags is "。" or ".";
    }

    private static bool IsHeaderLine(string line) =>
        IsKeywordOnlyLine(line) || IsCompactOpenerLine(line);

    private static void CollapseRunFromStart(List<string> lines)
    {
        while (lines.Count >= 2 && IsHeaderLine(lines[0]) && IsHeaderLine(lines[1]))
        {
            lines[0] = lines[0].TrimEnd() + lines[1].TrimStart();
            lines.RemoveAt(1);
        }
    }

    private static void CollapseRunFromEnd(List<string> lines)
    {
        while (lines.Count >= 2)
        {
            var last = lines.Count - 1;
            var prev = last - 1;
            if (!IsHeaderLine(lines[last]) || !IsHeaderLine(lines[prev]))
                break;

            lines[prev] = lines[prev].TrimEnd() + lines[last].TrimStart();
            lines.RemoveAt(last);
        }
    }
}
