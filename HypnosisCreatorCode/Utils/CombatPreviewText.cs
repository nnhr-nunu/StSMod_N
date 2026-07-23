using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 戦闘状況に依存する説明プレビュー（枚数・合計ダメージなど）を戦闘中だけ付与する。
/// 本家 <c>CalculatedVar.Calculate</c> は非戦闘時に倍率計算をスキップするため、
/// 説明文に埋め込むと「（0枚）」「(0 damage)」が残る。本modは括弧プレビューを戦闘中のみ足す。
/// </summary>
public static class CombatPreviewText
{
    public static bool IsActive(CardModel card)
    {
        try
        {
            if (!CombatManager.Instance.IsInProgress) return false;
        }
        catch
        {
            return false;
        }

        return card.CombatState != null;
    }

    public static void AppendSuffix(CardModel card, ref string description, string suffix)
    {
        if (!IsActive(card)) return;
        if (string.IsNullOrWhiteSpace(suffix)) return;
        if (description.Contains(suffix, StringComparison.Ordinal)) return;
        description = description.TrimEnd() + suffix;
    }

    /// <summary>プレビュー数値。基準値と異なるときだけ [green]（:diff() と同趣旨）。</summary>
    public static string FormatPreviewAmount(decimal preview, decimal baseline)
    {
        var culture = LocManager.Instance?.CultureInfo;
        var text = preview == decimal.Truncate(preview)
            ? ((int)preview).ToString(culture)
            : preview.ToString("0.##", culture);
        return preview != baseline ? $"[green]{text}[/green]" : text;
    }

    public static string FormatPreviewAmount(int preview, int baseline) =>
        FormatPreviewAmount((decimal)preview, (decimal)baseline);
}
