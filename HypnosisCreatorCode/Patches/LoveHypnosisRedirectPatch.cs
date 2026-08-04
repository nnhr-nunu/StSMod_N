using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 好き好き催眠 — 敵へのバフ付与後にプレイヤーへ移す（Postfix 奪取）。
/// async Task Postfix は本家の await チェーンに載らないため ref Task __result で続行する。
/// </summary>
public struct LoveHypnosisStealData
{
    public bool Active;
    public decimal Amount;
    public Creature? Player;
}

/// <summary>
/// 敵モンスター行動の <c>PowerCmd.Apply&lt;T&gt;(…, Creature, …)</c> 向け奪取。
/// 非ジェネリック <see cref="PowerCmd.Apply(PowerModel, …)"/> だけだと儀式の獣の筋力などが漏れる。
/// </summary>
[HarmonyPatch]
public static class LoveHypnosisApplyGenericStealPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.Method(typeof(PowerCmd), nameof(PowerCmd.Apply),
        [
            typeof(PlayerChoiceContext), typeof(Creature), typeof(decimal),
            typeof(Creature), typeof(CardModel), typeof(bool)
        ]);

    public static void Prefix(
        Creature target, decimal amount, Creature? applier, ref LoveHypnosisStealData __state)
    {
        __state = default;
        if (amount <= 0m) return;
        if (applier == null) return;
        if (target.Side != CombatSide.Enemy) return;
        if (!LoveHypnosisRedirect.TryGetActiveLoveHypnosis(applier, out var hypnosis) || !hypnosis.StealBuff)
            return;

        var player = hypnosis.ResolvePlayerCreature();
        if (player is not { IsAlive: true }) return;

        __state = new LoveHypnosisStealData
        {
            Active = true,
            Amount = amount,
            Player = player,
        };
    }

    public static void Postfix(
        ref Task __result,
        LoveHypnosisStealData __state,
        Creature? applier,
        CardModel? cardSource,
        PlayerChoiceContext choiceContext)
    {
        if (!__state.Active || __state.Player == null) return;
        var original = __result;
        __result = ContinueAfterGenericApply(original, __state, applier, cardSource, choiceContext);
    }

    private static async Task ContinueAfterGenericApply(
        Task original,
        LoveHypnosisStealData state,
        Creature? applier,
        CardModel? cardSource,
        PlayerChoiceContext choiceContext)
    {
        await original;

        var power = ExtractAppliedPower(original);
        if (power == null || power.Type != PowerType.Buff) return;
        if (!BuffStripRules.CanStripFromEnemy(power)) return;
        if (state.Amount <= 0m || state.Player == null) return;

        await LoveHypnosisRedirect.TransferBuffToPlayer(
            choiceContext, power, state.Amount, state.Player, applier, cardSource);
    }

    private static PowerModel? ExtractAppliedPower(Task task)
    {
        var taskType = task.GetType();
        if (!taskType.IsGenericType) return null;

        var result = taskType.GetProperty("Result")?.GetValue(task);
        return result as PowerModel;
    }
}

[HarmonyPatch]
public static class LoveHypnosisApplyStealPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.Method(typeof(PowerCmd), nameof(PowerCmd.Apply),
        [
            typeof(PlayerChoiceContext), typeof(PowerModel), typeof(Creature),
            typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)
        ]);

    public static void Prefix(
        PowerModel power, Creature target, decimal amount, Creature? applier,
        ref LoveHypnosisStealData __state)
    {
        __state = default;
        if (!LoveHypnosisRedirect.ShouldStealEnemyBuff(applier, target, power, out var player)) return;

        __state = new LoveHypnosisStealData
        {
            Active = true,
            Amount = amount,
            Player = player,
        };
    }

    public static void Postfix(
        ref Task __result,
        PowerModel power,
        LoveHypnosisStealData __state,
        Creature? applier,
        CardModel? cardSource,
        PlayerChoiceContext choiceContext)
    {
        if (!__state.Active || __state.Player == null) return;
        var original = __result;
        __result = ContinueAfterApply(original, power, __state, applier, cardSource, choiceContext);
    }

    private static async Task ContinueAfterApply(
        Task original,
        PowerModel power,
        LoveHypnosisStealData state,
        Creature? applier,
        CardModel? cardSource,
        PlayerChoiceContext choiceContext)
    {
        await original;
        if (state.Amount <= 0m || state.Player == null) return;

        await LoveHypnosisRedirect.TransferBuffToPlayer(
            choiceContext, power, state.Amount, state.Player, applier, cardSource);
    }
}

[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.ModifyAmount))]
public static class LoveHypnosisModifyAmountStealPatch
{
    public static void Prefix(PowerModel power, decimal offset, ref LoveHypnosisStealData __state)
    {
        __state = default;
        if (!LoveHypnosisRedirect.ShouldStealEnemyBuffAmount(power, offset, out var player)) return;

        __state = new LoveHypnosisStealData
        {
            Active = true,
            Amount = offset,
            Player = player,
        };
    }

    public static void Postfix(
        ref Task<int> __result,
        PowerModel power,
        LoveHypnosisStealData __state,
        Creature? applier,
        CardModel? cardSource,
        PlayerChoiceContext choiceContext)
    {
        if (!__state.Active || __state.Player == null) return;
        var original = __result;
        __result = ContinueAfterModifyAmount(original, power, __state, applier, cardSource, choiceContext);
    }

    private static async Task<int> ContinueAfterModifyAmount(
        Task<int> original,
        PowerModel power,
        LoveHypnosisStealData state,
        Creature? applier,
        CardModel? cardSource,
        PlayerChoiceContext choiceContext)
    {
        var result = await original;
        if (state.Amount <= 0m || state.Player == null) return result;

        await LoveHypnosisRedirect.TransferBuffToPlayer(
            choiceContext, power, state.Amount, state.Player, applier, cardSource);
        return result;
    }
}

[HarmonyPatch]
public static class LoveHypnosisGainBlockAmountPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.Method(typeof(CreatureCmd), nameof(CreatureCmd.GainBlock),
        [
            typeof(Creature), typeof(decimal), typeof(ValueProp), typeof(CardPlay), typeof(bool)
        ]);

    public static void Prefix(ref Creature creature)
    {
        if (LoveHypnosisRedirect.TryRetargetBlock(creature, out var redirected))
            creature = redirected;
    }
}

[HarmonyPatch]
public static class LoveHypnosisGainBlockVarPatch
{
    private static MethodBase TargetMethod() =>
        typeof(CreatureCmd).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(CreatureCmd.GainBlock)
                        && m.GetParameters().Length >= 3
                        && m.GetParameters()[1].ParameterType.Name == "BlockVar");

    public static void Prefix(ref Creature creature)
    {
        if (LoveHypnosisRedirect.TryRetargetBlock(creature, out var redirected))
            creature = redirected;
    }
}
