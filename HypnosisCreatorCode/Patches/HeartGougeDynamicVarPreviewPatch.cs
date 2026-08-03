using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 本家 <see cref="CardModel.UpdateDynamicVarPreview"/> は RunState 未設定のクローンで早期 return する。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.UpdateDynamicVarPreview))]
public static class HeartGougeUpdateDynamicVarPreviewPatch
{
    public static void Postfix(
        CardModel __instance,
        CardPreviewMode previewMode,
        Creature? target,
        DynamicVarSet dynamicVarSet)
    {
        _ = dynamicVarSet;
        HeartGougePreview.RefreshFromDynamicVarUpdate(__instance, previewMode, target);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForPile), [typeof(PileType), typeof(Creature)])]
public static class HeartGougeDescriptionPreviewPatch
{
    public static void Prefix(CardModel __instance, PileType pileType, Creature? target)
    {
        HeartGougePreview.EnterDescriptionContext(__instance);
        HeartGougePreview.RefreshForDescription(__instance, pileType, target);
    }

    public static void Postfix() => HeartGougePreview.LeaveDescriptionContext();
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForUpgradePreview))]
public static class HeartGougeUpgradeDescriptionPreviewPatch
{
    public static void Prefix(CardModel __instance)
    {
        HeartGougePreview.EnterDescriptionContext(__instance);
        HeartGougePreview.RefreshForDescription(__instance, PileType.None, null);
    }

    public static void Postfix() => HeartGougePreview.LeaveDescriptionContext();
}

/// <summary>
/// {Damage:diff()} 直前に簡易プレビューを再適用（他経路で PreviewValue=1 が入っても上書き）。
/// </summary>
[HarmonyPatch(typeof(DynamicVar), nameof(DynamicVar.ToHighlightedString))]
public static class HeartGougeDamageHighlightPatch
{
    public static void Prefix(DynamicVar __instance)
    {
        if (__instance.Name != "Damage") return;

        var card = HeartGougePreview.DescriptionFormattingCard;
        if (card == null || !HeartGougePreview.IsHeartGouge(card)) return;
        if (!ReferenceEquals(card.DynamicVars.Damage, __instance)) return;
        if (!HeartGougePreview.ShouldUseSimplifiedEnchantDisplay(card)) return;
        if (card.DynamicVars.Damage is not DamageVar damageVar) return;

        HeartGougePreview.ApplySimplifiedDamagePreview(card, damageVar);
    }
}
