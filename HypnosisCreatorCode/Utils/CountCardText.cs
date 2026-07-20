using BaseLib.Patches.Localization;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// カウントカードの説明重複を抑える。
/// Retain 機能自体は <see cref="Cards.HypnosisCreatorCard.CountKeywords"/> に残し、
/// ツールチップ（カウント説明の「保留。」）とダブる説明欄の自動挿入だけ消す。
/// </summary>
public static class CountCardText
{
    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += StripRedundantRetainText;

    private static void StripRedundantRetainText(CardModel card, Creature? target, ref string description)
    {
        if (!CountRules.HasCountKeyword(card)) return;

        // CardKeyword.Retain は beforeDescription へ [gold]保留[/gold]。 を自動挿入する。
        // カウントのキーワード説明がすでに「保留。」を含むため、説明欄だけ重複を外す。
        description = description
            .Replace("[gold]保留[/gold]。", "", StringComparison.Ordinal)
            .Replace("[gold]保留[/gold].", "", StringComparison.Ordinal)
            .Replace("[gold]Retain[/gold].", "", StringComparison.Ordinal)
            .Replace("[gold]Retain[/gold]。", "", StringComparison.Ordinal);
    }
}
