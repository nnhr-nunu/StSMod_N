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

    /// <summary>
    /// 多段の合計。本家 <c>LoseHpInternal</c> はヒットごとに <c>(int)</c>（0方向切り捨て）するため、
    /// 0.5×回数を先に足すと飛行・脱力で実ダメージ0なのに合計だけ残る。
    /// </summary>
    public static decimal SumRepeatedHits(decimal perHitAfterHooks, int hits)
    {
        if (hits <= 0) return 0m;
        return CombatPreviewText.RoundDisplayAmount(perHitAfterHooks) * hits;
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
        if (TryAppendCombinedDamageAndDoomSuffix(card, previewTarget, ref description, preview, raw))
            return;

        AppendDealDamageSuffix(card, ref description, preview, raw);
    }

    /// <summary>性癖刺さり時はダメージと破滅を1つの括弧にまとめる。</summary>
    private static bool TryAppendCombinedDamageAndDoomSuffix(
        CardModel card,
        Creature? target,
        ref string description,
        decimal preview,
        decimal baseline)
    {
        if (target is not { IsAlive: true, IsEnemy: true }) return false;

        var hits = FetishDoomPreview.CountPreviewHits(card, target);
        if (hits <= 0) return false;

        var perHit = FetishCombat.ScaleDoomByBog(
            target, FetishCombat.CalcFetishDoomAmount(target, card.Owner?.Creature));
        var doomTotal = perHit * hits;
        if (doomTotal <= 0) return false;

        var damageFormatted = CombatPreviewText.FormatPreviewAmount(preview, baseline);
        var doomFormatted = CombatPreviewText.FormatCombatPreviewAmount(doomTotal);
        var suffix = ModLocText.Get(
            "HYPNOSISCREATOR-PREVIEW_DAMAGE_AND_DOOM",
            $"（{damageFormatted}ダメージを与え、破滅{doomFormatted}を付与する）",
            $" ({damageFormatted} damage and Apply {doomFormatted} Doom)",
            loc =>
            {
                loc.Add("Damage", damageFormatted);
                loc.Add("Doom", doomFormatted);
            });

        description = FetishDoomPreview.StripDoomPreviewSuffix(description).TrimEnd();
        CombatPreviewText.AppendSuffix(card, ref description, suffix);
        return true;
    }

    public static void AppendDealDamageSuffix(
        CardModel card,
        ref string description,
        decimal preview,
        decimal baseline)
    {
        if (!CombatPreviewText.IsActive(card)) return;

        var formatted = CombatPreviewText.FormatPreviewAmount(preview, baseline);
        var suffix = ModLocText.Get(
            "HYPNOSISCREATOR-PREVIEW_DEAL_DAMAGE",
            $"（{formatted}ダメージを与える）",
            $" ({formatted} damage)",
            loc => loc.Add("Damage", formatted));
        CombatPreviewText.AppendSuffix(card, ref description, suffix);
    }

    public static void AppendTotalDamageSuffix(
        CardModel card,
        ref string description,
        decimal previewTotal,
        decimal baselineTotal)
    {
        if (!CombatPreviewText.IsActive(card)) return;
        if (previewTotal < 0m) return;
        if (previewTotal == 0m && baselineTotal <= 0m) return;

        var formatted = CombatPreviewText.FormatPreviewAmount(previewTotal, baselineTotal);
        var suffix = ModLocText.Get(
            "HYPNOSISCREATOR-PREVIEW_TOTAL_DAMAGE",
            $"（合計{formatted}ダメージ）",
            $" (Total {formatted} damage)",
            loc => loc.Add("Damage", formatted));
        CombatPreviewText.AppendSuffix(card, ref description, suffix);
    }

    public static void AppendBlockGainSuffix(CardModel card, ref string description, decimal preview, decimal baseline)
    {
        if (!CombatPreviewText.IsActive(card)) return;
        if (preview <= 0) return;

        var formatted = CombatPreviewText.FormatPreviewAmount(preview, baseline);
        var suffix = ModLocText.Get(
            "HYPNOSISCREATOR-PREVIEW_GAIN_BLOCK",
            $"（{formatted}ブロックを得る）",
            $" ({formatted} Block)",
            loc => loc.Add("Block", formatted));
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
        var suffix = CompactDamageSuffix(formatted);
        InsertBeforeExhaustOrAppend(card, ref description, suffix);
    }

    /// <summary>1ヒット値と連撃数（N×M）。補正なしの固定値表示向け。</summary>
    public static void AppendCompactPerHitDamageSuffix(
        CardModel card,
        ref string description,
        decimal perHit,
        int hits)
    {
        if (!CombatPreviewText.IsActive(card)) return;
        if (perHit <= 0 || hits <= 0) return;

        var formatted = CombatPreviewText.FormatAmount(perHit);
        var suffix = hits > 1
            ? ModLocText.Get(
                "HYPNOSISCREATOR-PREVIEW_DAMAGE_MULTI",
                $"（{formatted}×{hits}ダメージ）",
                $" ({formatted}×{hits} damage)",
                loc =>
                {
                    loc.Add("Damage", formatted);
                    loc.Add("Hits", hits);
                })
            : CompactDamageSuffix(formatted);
        InsertBeforeExhaustOrAppend(card, ref description, suffix);
    }

    private static string CompactDamageSuffix(string formatted) =>
        ModLocText.Get(
            "HYPNOSISCREATOR-PREVIEW_DAMAGE",
            $"（{formatted}ダメージ）",
            $" ({formatted} damage)",
            loc => loc.Add("Damage", formatted));

    private static void InsertBeforeExhaustOrAppend(CardModel card, ref string description, string suffix)
    {
        if (description.Contains(suffix, StringComparison.Ordinal)) return;

        if (HasExhaustKeyword(card))
        {
            var insertAt = FindExhaustKeywordIndex(description);
            if (insertAt >= 0)
            {
                var prefix = insertAt > 0 && description[insertAt - 1] == '\n' ? "" : "\n";
                description = description.Insert(insertAt, prefix + suffix);
                return;
            }
        }

        CombatPreviewText.AppendSuffix(card, ref description, suffix);
    }

    /// <summary>自動挿入された廃棄キーワード行の位置（表示言語のキーワード名で探す）。無ければ -1。</summary>
    private static int FindExhaustKeywordIndex(string description)
    {
        var localized = ModLocText.KeywordCardText("EXHAUST");
        if (localized != null)
        {
            var index = description.IndexOf(localized, StringComparison.Ordinal);
            if (index >= 0) return index;
        }

        var match = ExhaustKeywordToken.Match(description);
        return match.Success ? match.Index : -1;
    }

    private static bool HasExhaustKeyword(CardModel card) =>
        card.CanonicalKeywords.Contains(CardKeyword.Exhaust)
        || card.Keywords.Contains(CardKeyword.Exhaust);
}
