using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// <see cref="NEnchantPreview.Init"/> 中の <see cref="NCard.UpdateVisuals"/> で戦闘フックを走らせない。
/// </summary>
[HarmonyPatch(typeof(NEnchantPreview), "Init")]
public static class EnchantPreviewUiScopePatch
{
    public static void Prefix() => EnchantPreviewUiGuard.Push();

    public static void Finalizer() => EnchantPreviewUiGuard.Pop();
}
