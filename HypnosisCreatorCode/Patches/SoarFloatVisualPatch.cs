using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 本家 <see cref="SoarPower"/>（フクロウ判事の自己飛翔・好き好き催眠での奪取後も同クラス）向けに
/// 浮遊演出を差し込む。<see cref="SoarPower"/> は AfterApplied/AfterRemoved を上書きしていないため、
/// <see cref="PowerModel"/> の既定実装（何もしない no-op）を広くパッチして型で絞り込む。
/// 本mod独自の ThisTurnSoarPower 側は自身の AfterApplied/AfterRemoved から直接呼んでいるため対象外。
/// </summary>
[HarmonyPatch(typeof(PowerModel), nameof(PowerModel.AfterApplied))]
public static class VanillaSoarFloatApplyPatch
{
    public static void Postfix(PowerModel __instance, Creature? applier, CardModel? cardSource)
    {
        if (__instance is SoarPower && __instance.Owner is { IsAlive: true } owner)
            SoarFloatVisual.Begin(owner);
    }
}

[HarmonyPatch(typeof(PowerModel), nameof(PowerModel.AfterRemoved))]
public static class VanillaSoarFloatRemovePatch
{
    public static void Postfix(PowerModel __instance, Creature oldOwner)
    {
        if (__instance is SoarPower)
            SoarFloatVisual.End(oldOwner);
    }
}
