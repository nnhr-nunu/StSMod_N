using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 脳くちゅ催眠 — 攻撃＋デバフ行動で、ダメージだけ他敵へ寄ったあとデバフがプレイヤーに残るのを防ぐ。
/// UG時は攻撃付随デバフを相手全員へ広げる。
/// </summary>
[HarmonyPatch]
public static class BrainSlimeRedirectPowerModelApplyPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.DeclaredMethod(typeof(PowerCmd), nameof(PowerCmd.Apply),
        [
            typeof(PlayerChoiceContext), typeof(PowerModel), typeof(Creature),
            typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)
        ])!;

    public static void Prefix(
        Creature? applier,
        ref Creature target,
        ref BrainSlimeAoEDebuffState __state)
    {
        __state = default;
        if (BrainSlimeAoEDebuff.TryBegin(applier, ref target, out var state))
            __state = state;
        else
            BrainSlimeRedirectRules.TryRetarget(applier, ref target);
    }

    public static void Postfix(
        ref Task __result,
        BrainSlimeAoEDebuffState __state,
        PowerModel power,
        decimal amount,
        CardModel? cardSource,
        PlayerChoiceContext choiceContext)
    {
        if (!__state.Active) return;
        var original = __result;
        __result = BrainSlimeAoEDebuff.ContinueAfterApply(
            original, __state, power, amount, cardSource, choiceContext);
    }
}

[HarmonyPatch]
public static class BrainSlimeAoETargetsPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.Method(typeof(AttackCommand), "GetPossibleTargets")!;

    public static void Postfix(AttackCommand __instance, ref IReadOnlyList<Creature> __result)
    {
        if (!BrainSlimeAoETargeting.ShouldReplaceTargets(__instance)) return;
        var combat = __instance.Attacker?.CombatState;
        if (combat == null) return;
        __result = combat.HittableEnemies.Where(e => e is { IsAlive: true, IsEnemy: true }).ToList();
    }
}
