using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Config;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// カード絵更新のたびにクロップをテクスチャへ合わせる。
/// グリッドのホルダー使い回しで前のカードのクロップが残るのを防ぐ。
/// カードライブラリ内では本家表示のまま（何もしない）。
/// </summary>
[HarmonyPatch(typeof(NCard), "UpdatePortrait")]
public static class CardPortraitCropPatch
{
    public static void Postfix(NCard __instance)
    {
        if (CardLibraryUiGuard.IsUnderCardLibrary(__instance))
            return;

        VisualTuner.ApplyCardPortraitCrop(__instance);
    }
}
