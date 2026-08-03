using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

internal static class HeartGougePreview
{
    internal const string IdEntrySuffix = "HEART_GOUGE";

    internal static bool IsHeartGouge(CardModel card) =>
        card is HeartGouge || card.Id.Entry.EndsWith(IdEntrySuffix, StringComparison.Ordinal);

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

        var raw = damageVar.BaseValue;
        var props = damageVar.Props;

        if (card.IsEnchantmentPreview)
        {
            var enchanted = CardDamagePreview.ApplyEnchantmentModifiers(card, raw, props);
            damageVar.EnchantedValue = raw;
            damageVar.PreviewValue = enchanted;
            return;
        }

        var effectiveHooks = runGlobalHooks && !EnchantPreviewUiGuard.IsActive;
        var preview = card is HeartGouge gouge
            ? HeartGouge.ComputeDamagePreview(gouge, target, previewMode, effectiveHooks)
            : ComputeFallbackPreview(card, target, raw, props, previewMode, effectiveHooks);

        CardDamagePreview.SetDamagePreviewPair(card, damageVar, raw, preview, props);
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
