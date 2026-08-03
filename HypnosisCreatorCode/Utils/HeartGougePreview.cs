using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 心臓えぐり出しのダメージプレビュー。
/// イベント付与・デッキ詳細は弱体なしでエンチャント +50% 固定表示（本家フックが 1 になる経路を避ける）。
/// 戦闘中の手札／プレイは弱体先付与込みのフルプレビュー。
/// </summary>
internal static class HeartGougePreview
{
    internal const string IdEntrySuffix = "HEART_GOUGE";

    /// <summary>蝕み等のイベント／デッキ表示用固定倍率（CSV・本家 Corrupted と同じ 50%）。</summary>
    internal const decimal EventDeckEnchantMultiplier = 1.5m;

    private static int _descriptionContextDepth;
    private static CardModel? _descriptionCard;

    internal static bool IsHeartGouge(CardModel card) =>
        card is HeartGouge || card.Id.Entry.EndsWith(IdEntrySuffix, StringComparison.Ordinal);

    internal static void EnterDescriptionContext(CardModel card)
    {
        _descriptionContextDepth++;
        _descriptionCard = card;
    }

    internal static void LeaveDescriptionContext()
    {
        if (_descriptionContextDepth > 0)
            _descriptionContextDepth--;
        if (_descriptionContextDepth == 0)
            _descriptionCard = null;
    }

    internal static CardModel? DescriptionFormattingCard =>
        _descriptionContextDepth > 0 ? _descriptionCard : null;

    /// <summary>イベント・デッキ・エンチャント付与 UI — 戦闘フックと先付与弱体を使わない。</summary>
    internal static bool ShouldUseSimplifiedEnchantDisplay(CardModel card)
    {
        if (card.IsEnchantmentPreview) return true;
        if (EnchantPreviewUiGuard.IsActive) return true;
        if (card.CombatState == null) return true;

        var pile = card.Pile?.Type ?? PileType.None;
        return pile is not (PileType.Hand or PileType.Play);
    }

    internal static void RefreshFromDynamicVarUpdate(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target)
    {
        if (!IsHeartGouge(card)) return;
        if (card.DynamicVars.Damage is not DamageVar damageVar) return;

        ApplyDamageVarPreview(
            card,
            damageVar,
            previewMode,
            target,
            InferRunGlobalHooks(card));
    }

    internal static void RefreshForDescription(CardModel card, PileType pileType, Creature? target)
    {
        if (!IsHeartGouge(card)) return;
        if (card.DynamicVars.Damage is not DamageVar damageVar) return;

        ApplyDamageVarPreview(
            card,
            damageVar,
            CardPreviewMode.Normal,
            target,
            InferRunGlobalHooks(card, pileType));
    }

    internal static bool InferRunGlobalHooks(CardModel card, PileType? pileType = null)
    {
        if (card.CombatState == null) return false;

        var pile = pileType ?? card.Pile?.Type ?? PileType.None;
        if (pile is PileType.Hand or PileType.Play) return true;

        return card.UpgradePreviewType == CardUpgradePreviewType.Combat;
    }

    internal static void ApplyDamageVarPreview(
        CardModel card,
        DamageVar damageVar,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        if (!IsHeartGouge(card)) return;

        if (ShouldUseSimplifiedEnchantDisplay(card))
        {
            ApplySimplifiedDamagePreview(card, damageVar);
            return;
        }

        var raw = damageVar.BaseValue;
        var props = damageVar.Props;
        var effectiveHooks = runGlobalHooks && !EnchantPreviewUiGuard.IsActive;
        var preview = card is HeartGouge gouge
            ? HeartGouge.ComputeDamagePreview(gouge, target, previewMode, effectiveHooks)
            : ComputeFallbackPreview(card, target, raw, props, previewMode, effectiveHooks);

        CardDamagePreview.SetDamagePreviewPair(card, damageVar, raw, preview, props);
    }

    /// <summary>
    /// エンチャント付与確認の右カード: Enchanted=素値、Preview=+50%。
    /// エンチャント済みデッキ: 両方 +50% 後（差分なしで 22 等を表示）。
    /// 未エンチャント: 素値のみ。
    /// </summary>
    internal static void ApplySimplifiedDamagePreview(CardModel card, DamageVar damageVar)
    {
        var raw = damageVar.BaseValue;

        if (card.IsEnchantmentPreview)
        {
            damageVar.EnchantedValue = raw;
            damageVar.PreviewValue = ResolveEnchantedDisplayDamage(card, raw);
            return;
        }

        if (card.Enchantment != null)
        {
            var enchanted = ResolveEnchantedDisplayDamage(card, raw);
            damageVar.EnchantedValue = enchanted;
            damageVar.PreviewValue = enchanted;
            return;
        }

        damageVar.EnchantedValue = raw;
        damageVar.PreviewValue = raw;
    }

    internal static decimal ResolveEnchantedDisplayDamage(CardModel card, decimal raw)
    {
        if (card.Enchantment == null) return raw;

        return CombatPreviewText.RoundDisplayAmount(raw * EventDeckEnchantMultiplier);
    }

    private static decimal ComputeFallbackPreview(
        CardModel card,
        Creature? target,
        decimal raw,
        ValueProp props,
        CardPreviewMode previewMode,
        bool runGlobalHooks)
    {
        var enchanted = CardDamagePreview.ApplyEnchantmentModifiers(card, raw, props);
        if (!runGlobalHooks) return enchanted;

        return CardDamagePreview.ApplyModifiers(
            card, target, enchanted, props, previewMode, runGlobalHooks);
    }
}
