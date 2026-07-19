using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// カウントカードはコスト0、またはトランス条件を満たすときだけプレイ可。
/// 本家 <c>CanPlay(out UnplayableReason, out AbstractModel)</c> は out 引数のため
/// <see cref="ArgumentType.Out"/> で解決する（Ref だとパッチが掛からず制限が無効になる）。
/// </summary>
[HarmonyPatch(
    typeof(CardModel),
    nameof(CardModel.CanPlay),
    [typeof(UnplayableReason), typeof(AbstractModel)],
    [ArgumentType.Out, ArgumentType.Out])]
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

/// <summary>引数なし <see cref="CardModel.CanPlay()"/> 経由の保険。</summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.CanPlay), [])]
public static class CountUnplayableNoArgPatch
{
    public static void Postfix(CardModel __instance, ref bool __result)
    {
        if (!__result) return;
        if (!CountRules.HasCountKeyword(__instance)) return;
        if (CountRules.CanStartPlay(__instance)) return;
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
