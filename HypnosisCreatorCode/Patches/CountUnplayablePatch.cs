using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// カウントカードはコスト0、またはトランス条件を満たすときだけプレイ可。
/// <c>out</c> 引数は <see cref="Type.MakeByRefType"/> で解決する（ArgumentType 指定ミスでパッチ全体が死ぬのを防ぐ）。
/// </summary>
[HarmonyPatch]
public static class CountUnplayablePatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.Method(
            typeof(CardModel),
            nameof(CardModel.CanPlay),
            [typeof(UnplayableReason).MakeByRefType(), typeof(AbstractModel).MakeByRefType()])
        ?? throw new InvalidOperationException("CardModel.CanPlay(out UnplayableReason, out AbstractModel) not found");

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
