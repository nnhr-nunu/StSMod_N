using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

internal static class HeartGougePreview
{
    internal const string IdEntrySuffix = "HEART_GOUGE";

    internal static bool IsHeartGouge(CardModel card) =>
        card is HeartGouge || card.Id.Entry.EndsWith(IdEntrySuffix, StringComparison.Ordinal);

    internal static void ApplyDamageVarPreview(
        CardModel card,
        DamageVar damageVar,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        if (!IsHeartGouge(card)) return;

        var gouge = card as HeartGouge;
        if (gouge == null) return;

        var raw = gouge.DynamicVars.Damage.BaseValue;
        var props = damageVar.Props;

        if (gouge.IsEnchantmentPreview)
        {
            var enchanted = CardDamagePreview.ApplyEnchantmentModifiers(gouge, raw, props);
            damageVar.EnchantedValue = raw;
            damageVar.PreviewValue = enchanted;
            return;
        }

        var effectiveHooks = runGlobalHooks && !EnchantPreviewUiGuard.IsActive;
        var preview = HeartGouge.ComputeDamagePreview(gouge, target, previewMode, effectiveHooks);
        CardDamagePreview.SetDamagePreviewPair(gouge, damageVar, raw, preview, props);
    }
}
