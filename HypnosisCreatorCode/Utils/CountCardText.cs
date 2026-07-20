using System.Text.RegularExpressions;
using BaseLib.Patches.Localization;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// カウント／廃棄キーワードの説明重複を抑える。
/// Retain 機能自体は <see cref="Cards.HypnosisCreatorCard.CountKeywords"/> に残し、
/// ツールチップ（カウント説明の「保留。」）とダブる説明欄の自動挿入だけ消す。
/// Exhaust は CanonicalKeywords があるのに説明文末にも「廃棄。」を書いたときの二重表示を防ぐ。
/// </summary>
public static class CountCardText
{
    // キーワードは \n 区切りの1行。行ごと（末尾改行込み）消して空行を残さない。
    private static readonly Regex RetainKeywordLine = new(
        @"^\[gold\](?:保留|Retain)\[/gold\][。.](?:\r?\n)?",
        RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Multiline);

    // 本文末尾の素の「廃棄。」／"Exhaust."（キーワード自動挿入と二重になる分）
    private static readonly Regex TrailingPlainExhaust = new(
        @"(?:廃棄。|\s*Exhaust\.)\s*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static void Register()
    {
        DescriptionOverrides.CustomizeDescriptionPost += StripRedundantRetainText;
        DescriptionOverrides.CustomizeDescriptionPost += StripRedundantPlainExhaust;
    }

    private static void StripRedundantRetainText(CardModel card, Creature? target, ref string description)
    {
        if (!CountRules.HasCountKeyword(card)) return;

        // CardKeyword.Retain は beforeDescription へ [gold]保留[/gold]。 を自動挿入する。
        // カウントのキーワード説明がすでに「保留。」を含むため、説明欄だけ重複を外す。
        description = RetainKeywordLine.Replace(description, "");
    }

    private static void StripRedundantPlainExhaust(CardModel card, Creature? target, ref string description)
    {
        if (!card.CanonicalKeywords.Contains(CardKeyword.Exhaust)
            && !card.Keywords.Contains(CardKeyword.Exhaust))
            return;

        // CanonicalKeywords 由来の [gold]廃棄[/gold]。 が付くので、本文末尾の素の「廃棄。」は落とす
        description = TrailingPlainExhaust.Replace(description, "");
    }
}
