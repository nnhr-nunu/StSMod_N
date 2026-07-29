using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Ancient;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using HarmonyCard = HypnosisCreator.HypnosisCreatorCode.Cards.Basic.Harmony;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 古代の牙 — 調和（Harmony）を Agape へ超越登録。デッキに調和がないときは SetupForPlayer が false で抽選されない（本家同型）。
/// Agape は TranscendenceCards に入るため薄汚れた本の抽選対象外。
/// </summary>
[HarmonyPatch(typeof(ArchaicTooth), "get_TranscendenceUpgrades")]
public static class ArchaicToothTranscendencePatch
{
    public static void Postfix(Dictionary<ModelId, CardModel> __result)
    {
        __result[ModelDb.Card<HarmonyCard>().Id] = ModelDb.Card<Agape>();
    }
}
