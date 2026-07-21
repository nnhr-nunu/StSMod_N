using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// ヒプノクリエイターが付与した締め付けは「HPを失う」表記どおり、ブロックを無視して失わせる。
/// 本家（絞蛇等）の締め付けは従来どおりブロックで防げる。
/// </summary>
[HarmonyPatch(typeof(ConstrictPower), nameof(ConstrictPower.AfterSideTurnEnd))]
public static class ConstrictPowerHcUnblockablePatch
{
    public static bool Prefix(
        ConstrictPower __instance,
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants,
        ref Task __result)
    {
        _ = side;
        if (!ConstrictPowerHcText.ShouldUseHcText(__instance))
            return true;

        __result = ApplyUnblockableHpLoss(__instance, choiceContext, participants);
        return false;
    }

    private static async Task ApplyUnblockableHpLoss(
        ConstrictPower power,
        PlayerChoiceContext choiceContext,
        IEnumerable<Creature> participants)
    {
        if (power.Owner == null || !participants.Contains(power.Owner))
            return;

        await CreatureCmd.Damage(
            choiceContext,
            power.Owner,
            power.Amount,
            ValueProp.Unblockable | ValueProp.Unpowered,
            power.Applier ?? power.Owner);
    }
}
