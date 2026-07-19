using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

[HarmonyPatch]
public static class LoveHypnosisPowerApplySinglePatch
{
    private static MethodBase TargetMethod() =>
        typeof(PowerCmd).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m is { Name: nameof(PowerCmd.Apply), IsGenericMethodDefinition: true }
                        && m.GetParameters().Length == 6
                        && m.GetParameters()[1].ParameterType == typeof(Creature));

    public static void Prefix(ref Creature target, Creature applier)
    {
        if (LoveHypnosisRedirect.TryRetargetPower(applier, target, out var redirected))
            target = redirected;
    }
}

[HarmonyPatch]
public static class LoveHypnosisPowerApplyMultiPatch
{
    private static MethodBase TargetMethod() =>
        typeof(PowerCmd).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m is { Name: nameof(PowerCmd.Apply), IsGenericMethodDefinition: true }
                        && m.GetParameters().Length == 6
                        && m.GetParameters()[1].ParameterType != typeof(Creature));

    public static void Prefix(ref IEnumerable<Creature> targets, Creature applier)
    {
        var list = targets as Creature[] ?? targets.ToArray();
        var changed = false;
        for (var i = 0; i < list.Length; i++)
        {
            if (!LoveHypnosisRedirect.TryRetargetPower(applier, list[i], out var redirected)) continue;
            list[i] = redirected;
            changed = true;
        }

        if (changed)
            targets = list;
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

    public static void Prefix(ref Creature target, Creature applier)
    {
        if (LoveHypnosisRedirect.TryRetargetPower(applier, target, out var redirected))
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
