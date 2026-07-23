using System.Text.RegularExpressions;
using BaseLib.Patches.Localization;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 先頭・末尾で連続するキーワード専用行（性癖・カウント・廃棄・リプレイ等）を1行にまとめる。
/// 本文行（プレースホルダや素のテキストを含む行）は結合しない。
/// </summary>
public static class KeywordLineLayout
{
    private static readonly Regex ColorTag = new(
        @"\[(?:gold|blue|green)\][^\[]+\[/(?:gold|blue|green)\]",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += CollapseConsecutiveKeywordLines;

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

    private static void CollapseRunFromStart(List<string> lines)
    {
        while (lines.Count >= 2 && IsKeywordOnlyLine(lines[0]) && IsKeywordOnlyLine(lines[1]))
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
            if (!IsKeywordOnlyLine(lines[last]) || !IsKeywordOnlyLine(lines[prev]))
                break;

            lines[prev] = lines[prev].TrimEnd() + lines[last].TrimStart();
            lines.RemoveAt(last);
        }
    }
}
