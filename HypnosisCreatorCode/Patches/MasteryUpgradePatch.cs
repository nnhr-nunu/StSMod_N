using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>戦闘終了時: 練達が有効なプレイヤーの、戦闘中にプレイしたカウントカードを永続アップグレードする。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatEnd))]
public static class MasteryUpgradePatch
{
    public static void Postfix(IRunState runState, ICombatState? combatState, CombatRoom room)
    {
        if (combatState == null) return;

        foreach (var player in combatState.Players)
        {
            var mastery = player.Creature.GetPower<MasteryPower>();
            if (mastery == null) continue;

            foreach (var card in mastery.PlayedCountCards.ToList())
            {
                if (!card.IsUpgraded)
                    CardCmd.Upgrade(card);
            }
        }
    }
}
