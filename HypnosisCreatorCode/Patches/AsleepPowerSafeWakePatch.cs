using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// バニラ <see cref="AsleepPower"/> は起床時に <see cref="LagavulinMatriarch"/> へ cast する。
/// 本mod 付与分はビートルの <see cref="SlumberPower"/> に合わせ、ダメージ／ターン終了で1減らし、0で消す。
/// async フックは必ず <c>__result</c> に Task を返し、ターン進行の宙吊りを防ぐ。
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
        CardModel? cardSource,
        ref Task __result)
    {
        // ラガヴーリンは本家どおり（ダメージで即起床）
        if (__instance.Owner?.Monster is LagavulinMatriarch)
            return true;

        __result = SafeOnDamage(__instance, target, result);
        return false;
    }

    private static async Task SafeOnDamage(AsleepPower asleep, Creature target, DamageResult result)
    {
        if (target != asleep.Owner) return;
        if (result.UnblockedDamage <= 0) return;

        // ビートル（SlumberPower）同様: 1減るだけ。0になったら Decrement 側でパワー消滅
        await PowerCmd.Decrement(asleep);
        AsleepPowerWakeHelpers.NotifyVisual(asleep.Owner);
    }
}

/// <summary>
/// ターン終了時の減算。ラガヴーリン以外は cast せず Decrement のみ。
/// </summary>
[HarmonyPatch(typeof(AsleepPower), nameof(AsleepPower.AfterSideTurnEnd))]
public static class AsleepPowerTurnEndWakePatch
{
    public static bool Prefix(
        AsleepPower __instance,
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants,
        ref Task __result)
    {
        if (__instance.Owner?.Monster is LagavulinMatriarch)
            return true;

        __result = SafeOnTurnEnd(__instance, participants);
        return false;
    }

    private static async Task SafeOnTurnEnd(AsleepPower asleep, IEnumerable<Creature> participants)
    {
        var owner = asleep.Owner;
        if (owner == null || !participants.Contains(owner)) return;

        await PowerCmd.Decrement(asleep);
        AsleepPowerWakeHelpers.NotifyVisual(owner);
    }
}

static class AsleepPowerWakeHelpers
{
    public static void NotifyVisual(Creature? owner) =>
        owner?.GetPower<ForcedSleepVisualPower>()?.OnAsleepAmountMaybeChanged();
}
