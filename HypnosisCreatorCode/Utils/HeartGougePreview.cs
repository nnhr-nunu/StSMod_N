using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 心臓えぐり出しのダメージプレビュー。
/// イベント付与・デッキ詳細は弱体なし＋本家エンチャント計算のみ（Hook.ModifyDamage を避ける）。
/// 戦闘中の手札／プレイは弱体先付与込みのフルプレビュー。
/// </summary>
internal static class HeartGougePreview
{
    internal const string IdEntrySuffix = "HEART_GOUGE";

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

        if (card is not HeartGouge gouge) return;

        var raw = damageVar.BaseValue;
        var props = damageVar.Props;
        var effectiveHooks = runGlobalHooks && !EnchantPreviewUiGuard.IsActive;
        var preview = HeartGouge.ComputeDamagePreview(
            gouge, target, previewMode, effectiveHooks);
        CardDamagePreview.SetDamagePreviewPair(gouge, damageVar, raw, preview, props);
    }

    /// <summary>
    /// 戦闘外表示: 本家のエンチャント加算／倍率のみ（鋭利・蝕み等を個別に反映）。弱体先付与は含めない。
    /// </summary>
    internal static void ApplySimplifiedDamagePreview(CardModel card, DamageVar damageVar)
    {
        var raw = damageVar.BaseValue;
        var props = damageVar.Props;
        var enchanted = CardDamagePreview.ApplyEnchantmentModifiers(card, raw, props);

        if (card.Enchantment != null || card.IsEnchantmentPreview)
        {
            CardDamagePreview.SetDamagePreviewPair(card, damageVar, raw, enchanted, props);
            return;
        }

        CardDamagePreview.SetPreviewPair(damageVar, raw, raw);
    }
}
