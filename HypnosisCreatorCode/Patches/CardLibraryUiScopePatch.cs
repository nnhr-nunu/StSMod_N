using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// ライブラリの <see cref="NCard.UpdateVisuals"/> 中は mod の説明・キーワード改変を無効化する。
/// </summary>
[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
public static class CardLibraryUiScopePatch
{
    public static void Prefix(NCard __instance, ref bool __state)
    {
        __state = CardLibraryUiGuard.IsUnderCardLibrary(__instance);
        if (__state)
            CardLibraryUiGuard.Push();
    }

    /// <summary>
    /// Postfix だと本家が例外を投げたとき呼ばれず depth が上がったままになり、以後ずっと mod の表示改変が止まる。
    /// Finalizer は例外時も必ず走る（戻り値 void なので例外は握りつぶさない）。
    /// </summary>
    public static void Finalizer(bool __state)
    {
        if (__state)
            CardLibraryUiGuard.Pop();
    }
}
