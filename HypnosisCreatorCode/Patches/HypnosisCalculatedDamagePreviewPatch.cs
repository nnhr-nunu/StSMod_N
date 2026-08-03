using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// CalculatedDamageVar の本家 Calculate は CombatState 未設定時に倍率 0 になる。
/// ラン／手札／戦闘状況依存ダメージをプレビューで正しく出す。
/// </summary>
[HarmonyPatch(typeof(CalculatedDamageVar), "UpdateCardPreview")]
public static class HypnosisCalculatedDamagePreviewPatch
{
    public static void Postfix(
        CalculatedDamageVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        _ = runGlobalHooks;

        try
        {
            if (CardLibraryUiGuard.IsActive) return;

            ApplyPreview(__instance, card, previewMode, target, runGlobalHooks);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Calculated damage preview failed for {card.Id}: {ex.Message}");
        }
    }

    private static void ApplyPreview(
        CalculatedDamageVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        decimal? raw = card switch
        {
            Autopsy autopsy => autopsy.ComputeHeartScaledDamage(),
            MeetExpectations meet => MeetExpectations.ComputeDamage(meet),
            PerversionTrueDesire desire => PerversionTrueDesire.ComputeDamage(desire),
            MeltIntoTrance melt => MeltIntoTrance.ComputeDamage(melt, target ?? melt.CurrentTarget),
            VitalPoint vital => VitalPoint.ComputeDamage(vital),
            _ => null
        };
        if (raw == null) return;

        var baseDamage = raw.Value;

        if (card.IsEnchantmentPreview)
        {
            var enchantedPreview = CardDamagePreview.ApplyEnchantmentModifiers(card, baseDamage, ValueProp.Move);
            CardDamagePreview.SetDamagePreviewPair(card, __instance, baseDamage, enchantedPreview, ValueProp.Move);
            return;
        }

        var enchanted = CardDamagePreview.ApplyEnchantmentModifiers(card, baseDamage, ValueProp.Move);
        var preview = CardDamagePreview.ApplyModifiers(
            card, target, enchanted, ValueProp.Move, previewMode, runGlobalHooks);
        CardDamagePreview.SetDamagePreviewPair(card, __instance, baseDamage, preview, ValueProp.Move);
    }
}
