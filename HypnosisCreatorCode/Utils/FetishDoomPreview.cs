using System.Text.RegularExpressions;
using BaseLib.Patches.Localization;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 性癖カードを敵にドラッグ／照準中、説明末尾へ（破滅Nを付与する）を付ける。沼の1.5倍も反映。
/// タグ刺さりに加え、トランス付与によるトランス性癖刺さりも回数に含める。
/// </summary>
public static class FetishDoomPreview
{
    private static readonly Regex JapaneseDoomSuffix =
        new(@"（破滅.*を付与する）\s*$", RegexOptions.Compiled);

    private static readonly Regex EnglishDoomSuffix =
        new(@" \(Apply .* Doom\)\s*$", RegexOptions.Compiled);

    public static void Register() =>
        DescriptionOverrides.CustomizeDescriptionPost += AppendDoomPreviewSafely;

    private static void AppendDoomPreviewSafely(CardModel card, Creature? target, ref string description)
    {
        try
        {
            AppendDoomPreview(card, target, ref description);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Fetish doom preview failed for {card.Id}: {ex.Message}");
        }
    }

    private static void AppendDoomPreview(CardModel card, Creature? target, ref string description)
    {
        if (!CardFetishLookup.HasAnyFetish(card) && !CardFetishLookup.AppliesTrance(card)) return;

        var enemy = target ?? card.CurrentTarget;
        if (enemy is not { IsAlive: true, IsEnemy: true }) return;

        var hits = CountPreviewHits(card, enemy);
        if (hits <= 0) return;

        var perHit = FetishCombat.ScaleDoomByBog(
            enemy, FetishCombat.CalcFetishDoomAmount(enemy, card.Owner?.Creature));
        var total = perHit * hits;
        if (total <= 0) return;

        var formatted = CombatPreviewText.FormatCombatPreviewAmount(total);
        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（破滅{formatted}を付与する）"
            : $" (Apply {formatted} Doom)";

        description = StripDoomPreviewSuffix(description).TrimEnd() + suffix;
    }

    private static string StripDoomPreviewSuffix(string description) =>
        EnglishDoomSuffix.Replace(JapaneseDoomSuffix.Replace(description, string.Empty), string.Empty);

    /// <summary>照準対象に対して、実際の刺さりと同じ回数を返す。</summary>
    public static int CountPreviewHits(CardModel card, Creature target)
    {
        var tagHits = CountTagPreviewHits(card, target);
        var tranceHits = CountTranceApplyPreviewHits(card, target);
        return tagHits + tranceHits;
    }

    private static int CountTagPreviewHits(CardModel card, Creature target)
    {
        var fetishes = CardFetishLookup.GetFetishes(card);
        var alwaysHit = CardFetishLookup.AlwaysHitsFetish(card);
        if (fetishes.Count == 0 && !alwaysHit) return 0;

        List<FetishType> types;
        if (alwaysHit)
        {
            types = fetishes.Count > 0
                ? fetishes.Distinct().ToList()
                : [FetishType.Abnormal];
        }
        else
        {
            types = fetishes
                .Where(f => FetishCombat.HasFetish(target, f)
                            || (FetishCombat.CultLeaderActive && f != FetishType.Trance))
                .Distinct()
                .ToList();
        }

        if (types.Count == 0) return 0;
        return IsSingleHit(card) ? 1 : types.Count;
    }

    /// <summary>
    /// ApplyTrance 1回あたりトランス性癖刺さり1回（スタック量ではなく呼び出し回数）。
    /// プレビューは「このカードが1回プレイされたとき」の1回分。
    /// </summary>
    private static int CountTranceApplyPreviewHits(CardModel card, Creature target)
    {
        if (!CardFetishLookup.AppliesTrance(card)) return 0;
        return FetishCombat.HasFetish(target, FetishType.Trance) ? 1 : 0;
    }

    private static bool IsSingleHit(CardModel card)
    {
        if (card is Cards.HypnosisCreatorCard hc)
        {
            if (hc.FetishHitPerTypeOverride == true) return false;
            if (hc.FetishHitPerTypeOverride == false) return true;
            if (hc.AlwaysHitsFetish) return true;
            return hc.CardFetishes.Count <= 1;
        }

        return true;
    }
}
