using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 敵対象の状態異常／呪い：状態異常催眠＋トランス、または粘液の自由プレイ時のみ使用可。
/// </summary>
[HarmonyPatch(
    typeof(CardModel),
    nameof(CardModel.CanPlay),
    [typeof(UnplayableReason), typeof(AbstractModel)],
    [ArgumentType.Out, ArgumentType.Out])]
public static class StatusHypnosisUnplayablePatch
{
    public static void Postfix(CardModel __instance, ref bool __result)
    {
        if (!NeedsGate(__instance)) return;

        __result = StatusHypnosisRules.CanStartPlay(__instance);
    }

    internal static bool NeedsGate(CardModel card) =>
        card is AbnormalSlime || StatusHypnosisRules.IsGated(card);
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.CanPlay), [])]
public static class StatusHypnosisUnplayableNoArgPatch
{
    public static void Postfix(CardModel __instance, ref bool __result)
    {
        if (!StatusHypnosisUnplayablePatch.NeedsGate(__instance)) return;
        __result = StatusHypnosisRules.CanStartPlay(__instance);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.CanPlayTargeting))]
public static class StatusHypnosisCanPlayTargetingPatch
{
    public static void Postfix(CardModel __instance, Creature target, ref bool __result)
    {
        if (!StatusHypnosisUnplayablePatch.NeedsGate(__instance)) return;
        __result = StatusHypnosisRules.CanPlayTargeting(__instance, target);
    }
}
