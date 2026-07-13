using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

[HarmonyPatch(
    typeof(CardModel),
    nameof(CardModel.CanPlay),
    [typeof(UnplayableReason), typeof(AbstractModel)],
    [ArgumentType.Ref, ArgumentType.Ref]
)]
public static class CountUnplayablePatch
{
    public static void Postfix(
        CardModel __instance,
        ref bool __result,
        ref UnplayableReason reason)
    {
        if (!__result) return;
        if (!__instance.Keywords.Contains(HcKeywords.Count)) return;

        // GetResolved: 表示・支払いと同じ解決後コスト
        if (__instance.EnergyCost.GetResolved() != 0)
        {
            reason |= HcUnplayableReasons.CountNotZero;
            __result = false;
        }
    }
}
