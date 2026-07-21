using System.Threading.Tasks;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode;
using HypnosisCreator.HypnosisCreatorCode.Relics;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 戦闘終了時: ぜんぶ知ってるよ の刺さり倍率・教祖化の必中フラグを戦闘間で持ち越さないようにリセットする。
/// 布教欲求のゴールドは本家 Royalties と同じく報酬画面の追加 GoldReward として載せる。
/// 自己暗示などで得た一時レリックもここで除去する。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatEnd))]
public static class FetishCombatResetPatch
{
    /// <summary>
    /// <see cref="Hook.AfterCombatEnd"/> は async。通常の Postfix だと最初の await 時点で走るため、
    /// 完了後にゴールド報酬を載せる。
    /// </summary>
    public static void Postfix(
        ref Task __result,
        IRunState runState,
        ICombatState? combatState,
        CombatRoom room)
    {
        _ = runState;
        FetishCombat.FetishHitMultiplier = 1M;
        FetishCombat.CultLeaderActive = false;
        FetishCombat.ClearPlayScopes();
        EnemyPlayerAttackTracker.Reset();

        var original = __result;
        __result = ContinueAfterCombatEnd(original, combatState, room);
    }

    private static async Task ContinueAfterCombatEnd(
        Task original,
        ICombatState? combatState,
        CombatRoom room)
    {
        if (original != null)
            await original;

        if (combatState == null) return;

        foreach (var player in combatState.Players)
        {
            var gold = ProselytizeRewards.TakeGold(player);
            if (gold > 0)
            {
                // RoyaltiesPower.AfterCombatEnd と同じ UX（報酬画面にゴールド枠が出る）
                room.AddExtraReward(player, new GoldReward((int)gold, player, wasGoldStolenBack: false));
                MainFile.Logger.Info(
                    $"Proselytize GoldReward +{(int)gold} for {player.Character?.Id.Entry}");
            }

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

/// <summary>
/// 報酬生成直前にも布教ゴールドを載せる（AfterCombatEnd パッチ漏れの保険）。
/// </summary>
[HarmonyPatch(typeof(CombatRoom), nameof(CombatRoom.OfferRoomEndRewards))]
public static class ProselytizeGoldRewardPatch
{
    public static void Prefix(CombatRoom __instance)
    {
        var combat = __instance.CombatState;
        if (combat == null) return;

        foreach (var player in combat.Players)
        {
            var gold = ProselytizeRewards.TakeGold(player);
            if (gold <= 0) continue;

            __instance.AddExtraReward(player, new GoldReward((int)gold, player, wasGoldStolenBack: false));
            MainFile.Logger.Info(
                $"Proselytize GoldReward (offer) +{(int)gold} for {player.Character?.Id.Entry}");
        }
    }
}
