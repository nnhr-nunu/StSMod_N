using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
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
            var oshi = player.Creature.GetPower<OshiActivityPower>();
            if (oshi != null && oshi.GoldToGain > 0)
            {
                _ = PlayerCmd.GainGold(oshi.GoldToGain, player);
            }
        }
    }
}
