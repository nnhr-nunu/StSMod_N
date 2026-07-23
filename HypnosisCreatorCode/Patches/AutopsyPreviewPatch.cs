using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 解剖 — 心臓数倍率は戦闘外でも有効。本家 CalculatedVar.Calculate の戦闘中ガード後にプレビューを上書きする。
/// </summary>
[HarmonyPatch(typeof(CalculatedDamageVar), "UpdateCardPreview")]
public static class AutopsyPreviewPatch
{
    public static void Postfix(
        CalculatedDamageVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        _ = previewMode;
        _ = runGlobalHooks;
        if (card is not Autopsy autopsy) return;

        var raw = autopsy.ComputeHeartScaledDamage();
        __instance.EnchantedValue = raw;
        __instance.PreviewValue = autopsy.PreviewModifiedDamage(target, previewMode);
    }
}
