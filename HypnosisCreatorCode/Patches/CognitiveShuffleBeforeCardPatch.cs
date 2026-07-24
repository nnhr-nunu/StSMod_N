using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 認知シャッフル付与完了前に次カードを打つと PowerCmd が競合して宙吊りするため待機する。
/// async フックは Postfix で <c>__result</c> を延長する（Prefix では Task が未設定のことがある）。
/// 期限切れは <see cref="Powers.CognitiveShufflePower.AfterCardPlayed"/> で処理（暗示解除との競合回避）。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
public static class CognitiveShuffleBeforeCardPatch
{
    [HarmonyPriority(Priority.First)]
    public static void Postfix(ref Task __result, ICombatState combatState, CardPlay cardPlay)
    {
        _ = combatState;
        if (cardPlay.Card is CognitiveShuffle) return;

        var wait = CognitiveShuffleCompletion.WaitForGateAsync(cardPlay.Card.Owner);
        if (wait.IsCompletedSuccessfully) return;

        var original = __result;
        __result = ContinueAfterGateAsync(original, wait);
    }

    private static async Task ContinueAfterGateAsync(Task? original, Task wait)
    {
        await wait;
        if (original != null)
            await original;
    }
}

/// <summary>虚無化などで連続プレイ時、付与完了前のクリック自体を不可にする。</summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.CanPlay), [])]
public static class CognitiveShuffleGateCanPlayPatch
{
    public static void Postfix(CardModel __instance, ref bool __result)
    {
        if (!__result) return;
        if (__instance is CognitiveShuffle) return;
        if (CognitiveShuffleCompletion.IsGateActive(__instance.Owner))
            __result = false;
    }
}

[HarmonyPatch(
    typeof(CardModel),
    nameof(CardModel.CanPlay),
    [typeof(UnplayableReason), typeof(AbstractModel)],
    [ArgumentType.Out, ArgumentType.Out])]
public static class CognitiveShuffleGateCanPlayReasonPatch
{
    public static void Postfix(CardModel __instance, ref bool __result)
    {
        if (!__result) return;
        if (__instance is CognitiveShuffle) return;
        if (CognitiveShuffleCompletion.IsGateActive(__instance.Owner))
            __result = false;
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.CanPlayTargeting))]
public static class CognitiveShuffleGateCanPlayTargetingPatch
{
    public static void Postfix(CardModel __instance, Creature target, ref bool __result)
    {
        _ = target;
        if (!__result) return;
        if (__instance is CognitiveShuffle) return;
        if (CognitiveShuffleCompletion.IsGateActive(__instance.Owner))
            __result = false;
    }
}
