using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Rewards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// デトックス鍛冶報酬は <see cref="RewardType.SpecialCard"/> 表示だがカード実体を持たない。
/// 本家 <see cref="Reward.FromSerializable"/> は <see cref="SerializableReward.SpecialCard"/> を復元するため、
/// 未登録だと続きからで NRE になる。
/// </summary>
[HarmonyPatch(typeof(Reward), nameof(Reward.FromSerializable))]
internal static class DetoxDeckUpgradeRewardDeserializePatch
{
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(SerializableReward save, Player player, ref Reward __result)
    {
        if (save.RewardType != RewardType.SpecialCard || save.SpecialCard != null)
            return true;

        __result = new DetoxDeckUpgradeReward(player);
        return false;
    }
}
