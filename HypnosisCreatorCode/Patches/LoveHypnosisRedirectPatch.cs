using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 好き好き催眠 — PowerCmd.Apply の付け替え。
/// ジェネリック定義への Harmony パッチは失敗するため、バフ型ごとの構築済みメソッドを手動登録する。
/// </summary>
public static class LoveHypnosisRedirectPatcher
{
    private static readonly Type[] BuffApplyTypes = [
        typeof(StrengthPower),
        typeof(DexterityPower),
        typeof(PlatingPower),
        typeof(ArtifactPower),
        typeof(VigorPower),
        typeof(ThornsPower),
        typeof(CurlUpPower),
        typeof(TerritorialPower),
        typeof(IntangiblePower),
        typeof(BlurPower),
        typeof(DrawCardsNextTurnPower),
        typeof(EnergyNextTurnPower),
        typeof(RegenPower),
        typeof(RagePower),
        typeof(DemonFormPower),
    ];

    public static void Apply(Harmony harmony, MegaCrit.Sts2.Core.Logging.Logger logger)
    {
        var genericDef = typeof(PowerCmd).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m is { Name: nameof(PowerCmd.Apply), IsGenericMethodDefinition: true }
                        && m.GetParameters().Length == 6
                        && m.GetParameters()[1].ParameterType == typeof(Creature));

        var prefix = AccessTools.Method(typeof(LoveHypnosisRedirectPatcher), nameof(GenericApplyPrefix));

        foreach (var powerType in BuffApplyTypes)
        {
            try
            {
                var method = genericDef.MakeGenericMethod(powerType);
                harmony.Patch(method, new HarmonyMethod(prefix));
            }
            catch (Exception ex)
            {
                logger.Warn($"LoveHypnosis buff Apply patch failed for {powerType.Name}: {ex.Message}");
            }
        }
    }

    public static void GenericApplyPrefix(ref Creature target, Creature? applier)
    {
        if (LoveHypnosisRedirect.TryRetargetPower(applier, target, out var redirected))
            target = redirected;
    }
}

/// <summary>
/// 好き好き催眠 — 敵が既に対象バフを持っている場合の増量パッチ。
/// return false + async __result は戦闘ループを塞ぎ進行不能になるため、
/// 敵への加算を 0 にしてから非同期でプレイヤーへ付与する。
/// </summary>
[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.ModifyAmount))]
public static class LoveHypnosisModifyAmountPatch
{
    private static readonly MethodInfo GenericApply = typeof(PowerCmd)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m is { Name: nameof(PowerCmd.Apply), IsGenericMethodDefinition: true }
                    && m.GetParameters().Length == 6
                    && m.GetParameters()[1].ParameterType == typeof(Creature));

    public struct StealData
    {
        public bool Active;
        public decimal Amount;
        public Creature? Player;
        public Type PowerType;
    }

    public static void Prefix(PowerModel power, ref decimal offset, ref StealData __state)
    {
        __state = default;
        if (offset <= 0m) return;
        if (!LoveHypnosisRedirect.TryGetStealingBuffOwner(power, out var player)) return;

        __state = new StealData
        {
            Active = true,
            Amount = offset,
            Player = player,
            PowerType = power.GetType(),
        };
        offset = 0m;
    }

    public static void Postfix(
        PlayerChoiceContext choiceContext,
        Creature? applier,
        CardModel? cardSource,
        bool silent,
        StealData __state)
    {
        if (!__state.Active || __state.Player == null || __state.Amount <= 0m) return;

        TaskHelper.RunSafely(ApplyStolenAsync(
            choiceContext, __state.PowerType, __state.Player, __state.Amount, applier, cardSource, silent));
    }

    private static async Task ApplyStolenAsync(
        PlayerChoiceContext choiceContext, Type powerType, Creature player, decimal amount,
        Creature? applier, CardModel? cardSource, bool silent)
    {
        var apply = GenericApply.MakeGenericMethod(powerType);
        var task = (Task)apply.Invoke(null, [choiceContext, player, amount, applier, cardSource, silent])!;
        await task;
    }
}

[HarmonyPatch]
public static class LoveHypnosisPowerApplyModelPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.Method(typeof(PowerCmd), nameof(PowerCmd.Apply),
        [
            typeof(PlayerChoiceContext), typeof(PowerModel), typeof(Creature),
            typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)
        ]);

    public static void Prefix(ref Creature target, Creature? applier, PowerModel power)
    {
        if (LoveHypnosisRedirect.TryRetargetPower(applier, target, power, out var redirected))
            target = redirected;
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
