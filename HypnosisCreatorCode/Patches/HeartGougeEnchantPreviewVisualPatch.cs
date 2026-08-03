using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// エンチャント付与確認の <see cref="NCard.UpdateVisuals"/> 完了後に心臓えぐり出しの Damage プレビューを再適用する。
/// 本家が先に PreviewValue=1 を入れる経路を最終的に上書きする。
/// </summary>
[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
public static class HeartGougeEnchantPreviewVisualPatch
{
    public static void Postfix(NCard __instance)
    {
        if (!EnchantPreviewUiGuard.IsUnderEnchantPreview(__instance)) return;

        var card = __instance.Model;
        if (card == null || !HeartGougePreview.IsHeartGouge(card)) return;
        if (card.DynamicVars.Damage is not DamageVar damageVar) return;

        HeartGougePreview.ApplyDamageVarPreview(
            card,
            damageVar,
            CardPreviewMode.Normal,
            card.CurrentTarget,
            runGlobalHooks: false);
    }
}
