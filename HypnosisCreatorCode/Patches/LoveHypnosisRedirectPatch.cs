using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
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
/// Prefix で着弾先を差し替えると敵行動中の CustomScaledWait で進行不能になる。
/// </summary>
public struct LoveHypnosisStealData
{
    public bool Active;
    public decimal Amount;
    public Creature? Player;
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

    public static async Task Postfix(
        Task __result,
        PowerModel power,
        LoveHypnosisStealData __state,
        Creature? applier,
        CardModel? cardSource,
        PlayerChoiceContext choiceContext)
    {
        await __result;
        if (!__state.Active || __state.Player == null || __state.Amount <= 0m) return;

        await LoveHypnosisRedirect.TransferBuffToPlayer(
            choiceContext, power, __state.Amount, __state.Player, applier, cardSource);
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

    public static async Task Postfix(
        Task<int> __result,
        PowerModel power,
        LoveHypnosisStealData __state,
        Creature? applier,
        CardModel? cardSource,
        PlayerChoiceContext choiceContext)
    {
        await __result;
        if (!__state.Active || __state.Player == null || __state.Amount <= 0m) return;

        await LoveHypnosisRedirect.TransferBuffToPlayer(
            choiceContext, power, __state.Amount, __state.Player, applier, cardSource);
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
