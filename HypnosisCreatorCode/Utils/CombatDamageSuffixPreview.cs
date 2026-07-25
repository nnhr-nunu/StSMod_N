using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 戦闘中のみ説明末尾へダメージ／ブロック括弧を追記する。弱体・筋力は <see cref="CardDamagePreview"/> 経由。
/// </summary>
public static class CombatDamageSuffixPreview
{
    private static readonly Regex ExhaustKeywordToken = new(
        @"\[gold\](?:廃棄|Exhaust)\[/gold\][。.]",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static IReadOnlyList<Creature> GetHittableEnemies(CardModel card) =>
        card.CombatState?.HittableEnemies
            .Where(e => e.IsAlive && e.IsEnemy)
            .ToList()
        ?? [];

    /// <summary>
    /// 全体攻撃の1ヒットあたり。全員が同じデバフで実効が変わるときだけ敵側補正を採用し、混在時はベース（プレイヤー側補正のみ）に寄せる。
    /// </summary>
    public static decimal ResolveAoEPerHit(CardModel card, decimal raw, ValueProp props)
    {
        var enemies = GetHittableEnemies(card);
        if (enemies.Count == 0)
            return CardDamagePreview.ApplyModifiers(card, null, raw, props);

        var previews = enemies
            .Select(e => CardDamagePreview.ApplyModifiers(card, e, raw, props))
            .ToList();

        if (previews.TrueForAll(p => p != raw) && previews.Distinct().Count() == 1)
            return previews[0];

        return CardDamagePreview.ApplyModifiers(card, null, raw, props);
    }

    public static void AppendDealDamageSuffix(
        CardModel card,
        Creature? target,
        ref string description,
        decimal raw,
        ValueProp props)
    {
        if (!CombatPreviewText.IsActive(card)) return;
        if (raw <= 0) return;

        var previewTarget = target ?? card.CurrentTarget;
        var preview = CardDamagePreview.ApplyModifiers(card, previewTarget, raw, props);
        AppendDealDamageSuffix(card, ref description, preview, raw);
    }

    public static void AppendDealDamageSuffix(
        CardModel card,
        ref string description,
        decimal preview,
        decimal baseline)
    {
        if (!CombatPreviewText.IsActive(card)) return;

        var formatted = CombatPreviewText.FormatPreviewAmount(preview, baseline);
        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（{formatted}ダメージを与える）"
            : $" ({formatted} damage)";
        CombatPreviewText.AppendSuffix(card, ref description, suffix);
    }

    public static void AppendTotalDamageSuffix(
        CardModel card,
        ref string description,
        decimal previewTotal,
        decimal baselineTotal)
    {
        if (!CombatPreviewText.IsActive(card)) return;
        if (previewTotal <= 0) return;

        var formatted = CombatPreviewText.FormatPreviewAmount(previewTotal, baselineTotal);
        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（合計{formatted}ダメージ）"
            : $" (Total {formatted} damage)";
        CombatPreviewText.AppendSuffix(card, ref description, suffix);
    }

    public static void AppendBlockGainSuffix(CardModel card, ref string description, decimal preview, decimal baseline)
    {
        if (!CombatPreviewText.IsActive(card)) return;
        if (preview <= 0) return;

        var formatted = CombatPreviewText.FormatPreviewAmount(preview, baseline);
        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（{formatted}ブロックを得る）"
            : $" ({formatted} Block)";
        CombatPreviewText.AppendSuffix(card, ref description, suffix);
    }

    /// <summary>戦闘中のみ（Nダメージ）を付与。廃棄キーワード行の直前に挿入（無ければ末尾）。</summary>
    public static void AppendCompactDealDamageSuffix(
        CardModel card,
        Creature? target,
        ref string description,
        decimal raw,
        ValueProp props)
    {
        if (!CombatPreviewText.IsActive(card)) return;
        if (raw <= 0) return;

        var previewTarget = target ?? card.CurrentTarget;
        var preview = CardDamagePreview.ApplyModifiers(card, previewTarget, raw, props);
        AppendCompactDealDamageSuffix(card, ref description, preview, raw);
    }

    public static void AppendCompactDealDamageSuffix(
        CardModel card,
        ref string description,
        decimal preview,
        decimal baseline)
    {
        if (!CombatPreviewText.IsActive(card)) return;

        var formatted = CombatPreviewText.FormatPreviewAmount(preview, baseline);
        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（{formatted}ダメージ）"
            : $" ({formatted} damage)";
        InsertBeforeExhaustOrAppend(card, ref description, suffix);
    }

    private static void InsertBeforeExhaustOrAppend(CardModel card, ref string description, string suffix)
    {
        if (description.Contains(suffix, StringComparison.Ordinal)) return;

        if (HasExhaustKeyword(card))
        {
            var match = ExhaustKeywordToken.Match(description);
            if (match.Success)
            {
                var insertAt = match.Index;
                var prefix = insertAt > 0 && description[insertAt - 1] == '\n' ? "" : "\n";
                description = description.Insert(insertAt, prefix + suffix);
                return;
            }
        }

        CombatPreviewText.AppendSuffix(card, ref description, suffix);
    }

    private static bool HasExhaustKeyword(CardModel card) =>
        card.CanonicalKeywords.Contains(CardKeyword.Exhaust)
        || card.Keywords.Contains(CardKeyword.Exhaust);
}
