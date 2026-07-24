using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 戦闘中のみ説明末尾へダメージ／ブロック括弧を追記する。弱体・筋力は <see cref="CardDamagePreview"/> 経由。
/// </summary>
public static class CombatDamageSuffixPreview
{
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
}
