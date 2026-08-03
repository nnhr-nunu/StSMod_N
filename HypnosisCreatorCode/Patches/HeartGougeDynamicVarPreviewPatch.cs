using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 本家 <see cref="CardModel.UpdateDynamicVarPreview"/> は RunState 未設定のクローンで早期 return し
/// <see cref="DamageVar.UpdateCardPreview"/> パッチが走らない。Postfix で必ず心臓えぐり出しを補正する。
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

/// <summary>
/// デッキ詳細等、説明文だけ更新する経路でもプレビューが古い 1 ダメージのまま残らないよう直前で補正する。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForPile), [typeof(PileType), typeof(Creature)])]
public static class HeartGougeDescriptionPreviewPatch
{
    public static void Prefix(CardModel __instance, PileType pileType, Creature? target) =>
        HeartGougePreview.RefreshForDescription(__instance, pileType, target);
}
