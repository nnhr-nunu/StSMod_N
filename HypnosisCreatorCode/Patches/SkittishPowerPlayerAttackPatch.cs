using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Reflection;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 本家 <see cref="SkittishPower"/> は攻撃元がカードのときだけブロックを付与する。
/// プレイヤーが臆病を得た場合（幻影蟲の心臓等）は敵モンスター行動でも反応させる。
/// </summary>
[HarmonyPatch(typeof(SkittishPower), nameof(SkittishPower.AfterAttack))]
public static class SkittishPowerPlayerAttackPatch
{
    private static readonly MethodInfo SetHasGainedBlockThisTurn =
        AccessTools.PropertySetter(typeof(SkittishPower), nameof(SkittishPower.HasGainedBlockThisTurn))!;

    public static bool Prefix(
        SkittishPower __instance,
        PlayerChoiceContext choiceContext,
        AttackCommand command,
        ref Task __result)
    {
        if (__instance.Owner?.Player == null) return true;

        __result = GainBlockOnFirstHit(__instance, command);
        return false;
    }

    private static async Task GainBlockOnFirstHit(SkittishPower power, AttackCommand command)
    {
        if (power.HasGainedBlockThisTurn) return;
        if (!command.DamageProps.HasFlag(ValueProp.Move)) return;

        var hit = command.Results
            .SelectMany(results => results)
            .FirstOrDefault(result => result.Receiver == power.Owner);
        if (hit == null || hit.UnblockedDamage <= 0) return;

        SetHasGainedBlockThisTurn.Invoke(power, [true]);
        SfxCmd.Play("event:/sfx/enemy/enemy_attacks/phantasmal_gardeners/phantasmal_gardeners_retract", 1f);
        await CreatureCmd.TriggerAnim(power.Owner, "BlockStart", 0.3f);
        await CreatureCmd.GainBlock(power.Owner, power.Amount, ValueProp.Unpowered, null);
    }
}
