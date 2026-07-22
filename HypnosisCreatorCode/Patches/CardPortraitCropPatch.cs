using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Config;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// カード絵更新のたびにクロップをテクスチャへ合わせる。
/// グリッドのホルダー使い回しで前のカードのクロップが残るのを防ぐ。
/// カードライブラリ内ではクロップを外し本家表示にする。
/// </summary>
[HarmonyPatch(typeof(NCard), "UpdatePortrait")]
public static class CardPortraitCropPatch
{
    public static void Postfix(NCard __instance) =>
        VisualTuner.ApplyCardPortraitCrop(__instance);
}
