using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>敵がプレイヤーにデバフを付与したとき、付与元を記録する。</summary>
[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply),
    [typeof(PlayerChoiceContext), typeof(PowerModel), typeof(Creature), typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)])]
public static class DebuffSourcePatch
{
    public static void Postfix(PowerModel power, Creature target, Creature applier)
    {
        if (power.Type != PowerType.Debuff) return;
        DebuffSourceTracker.Record(target, applier);
    }
}
