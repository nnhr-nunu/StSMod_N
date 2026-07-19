using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// バニラ <see cref="AsleepPower"/> の起床処理は <see cref="LagavulinMatriarch"/> 前提の cast があり、
/// 他モンスター（寝かしつけ催眠など）では起床時に例外になりうる。ラガヴーリン以外は安全に起床させる。
/// </summary>
[HarmonyPatch(typeof(AsleepPower), nameof(AsleepPower.AfterDamageReceived))]
public static class AsleepPowerDamageWakePatch
{
    public static bool Prefix(
        AsleepPower __instance,
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (__instance.Owner?.Monster is LagavulinMatriarch)
            return true;

        _ = SafeWakeFromDamage(__instance, target, result);
        return false;
    }

    private static async Task SafeWakeFromDamage(AsleepPower asleep, Creature target, DamageResult result)
    {
        if (target != asleep.Owner) return;
        if (result.UnblockedDamage <= 0) return;

        var owner = asleep.Owner;
        if (owner == null) return;

        var plating = owner.GetPower<PlatingPower>();
        if (plating != null)
            await PowerCmd.Remove(plating);

        if (owner.Monster is SlumberingBeetle beetle)
        {
            await beetle.WakeUpMove(Array.Empty<Creature>());
            return;
        }

        if (owner.HasPower<AsleepPower>())
            await PowerCmd.Remove(asleep);
    }
}

/// <summary>
/// 睡眠スタック切れ時もラガヴーリン専用 WakeUpMove へ cast しない。
/// </summary>
[HarmonyPatch(typeof(AsleepPower), nameof(AsleepPower.AfterSideTurnEnd))]
public static class AsleepPowerTurnEndWakePatch
{
    public static bool Prefix(
        AsleepPower __instance,
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (__instance.Owner?.Monster is LagavulinMatriarch)
            return true;

        _ = SafeDecrement(__instance, participants);
        return false;
    }

    private static async Task SafeDecrement(AsleepPower asleep, IEnumerable<Creature> participants)
    {
        var owner = asleep.Owner;
        if (owner == null || !participants.Contains(owner)) return;

        await PowerCmd.Decrement(asleep);

        if (asleep.Amount > 0) return;

        if (owner.Monster is SlumberingBeetle beetle)
            await beetle.WakeUpMove(Array.Empty<Creature>());
    }
}
