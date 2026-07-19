using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>他色アブノーマル対象カードも、刺さりうるとき黄ハイライトする。</summary>
[HarmonyPatch(typeof(CardModel), "get_ShouldGlowGoldInternal")]
public static class AbnormalOtherColorGlowPatch
{
    public static void Postfix(CardModel __instance, ref bool __result)
    {
        if (__result) return;
        if (!AbnormalOtherColorPool.Contains(__instance)) return;
        __result = FetishGlow.ShouldGlow(__instance);
    }
}
