using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 心臓レリックのホバー説明直前に、ダメージ／ブロックのプレビュー数値を更新する。
/// </summary>
[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.HoverTip), MethodType.Getter)]
public static class HeartRelicPreviewPatch
{
    [HarmonyPrefix]
    public static void Prefix(RelicModel __instance) =>
        HeartRelicPreview.Refresh(__instance);
}
