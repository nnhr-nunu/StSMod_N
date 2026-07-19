using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Relics;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 戦闘終了時: ぜんぶ知ってるよ の刺さり倍率・教祖化の必中フラグを戦闘間で持ち越さないようにリセットする。
/// また、布教欲求で得られるゴールドをここで清算する。
/// 自己暗示などで得た一時レリックもここで除去する。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatEnd))]
public static class FetishCombatResetPatch
{
    public static void Postfix(IRunState runState, ICombatState? combatState, CombatRoom room)
    {
        FetishCombat.FetishHitMultiplier = 1M;
        FetishCombat.CultLeaderActive = false;
        EnemyPlayerAttackTracker.Reset();

        if (combatState == null) return;
        foreach (var player in combatState.Players)
        {
            var gold = ProselytizeRewards.TakeGold(player.Creature);
            if (gold > 0)
                _ = PlayerCmd.GainGold(gold, player);

            // 自己暗示など「この戦闘中」の一時レリックを除去
            var temps = player.Relics
                .OfType<HypnosisCreatorRelic>()
                .Where(r => r.RemoveAtCombatEnd)
                .ToList();
            foreach (var relic in temps)
                _ = RelicCmd.Remove(relic);
        }
    }
}
