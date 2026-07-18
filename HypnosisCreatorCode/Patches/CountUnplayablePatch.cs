using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
        if (!CountRules.HasCountKeyword(__instance)) return;
        if (CountRules.CanStartPlay(__instance)) return;

        reason |= CustomEnums.HcUnplayableReasons.CountNotZero;
        __result = false;
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.CanPlayTargeting))]
public static class CountCanPlayTargetingPatch
{
    public static void Postfix(CardModel __instance, Creature target, ref bool __result)
    {
        if (!__result) return;

        if (__instance is Cards.Basic.Mirroring && !Cards.Basic.Mirroring.HasAttackIntent(target))
        {
            __result = false;
            return;
        }

        if (!CountRules.HasCountKeyword(__instance)) return;
        if (CountRules.CanPlayTargeting(__instance, target)) return;
        __result = false;
    }
}
