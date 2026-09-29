using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Rewards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// デトックス鍛冶報酬の復元。本家 <see cref="Reward.FromSerializable"/> は RemoveCard を本家カード削除報酬として、
/// 旧形式（SpecialCard でカード実体なし）は <see cref="SerializableReward.SpecialCard"/> を復元しようとして NRE になるため先に拾う。
/// </summary>
[HarmonyPatch(typeof(Reward), nameof(Reward.FromSerializable))]
internal static class DetoxDeckUpgradeRewardDeserializePatch
{
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(SerializableReward save, Player player, ref Reward __result)
    {
        if (!DetoxDeckUpgradeReward.IsSaved(save))
            return true;

        __result = new DetoxDeckUpgradeReward(player);
        return false;
    }
}

/// <summary>
/// 旧形式（SpecialCard でカード実体なし）のまま共闘のセーブ読込で送信されると、本家
/// <see cref="SerializableReward.Serialize"/> が null のカードを書こうとして例外になる。送信直前に新形式へ直す。
/// </summary>
[HarmonyPatch(typeof(SerializableReward), nameof(SerializableReward.Serialize))]
internal static class DetoxDeckUpgradeRewardSerializePatch
{
    private static void Prefix(SerializableReward __instance)
    {
        if (__instance.RewardType != RewardType.SpecialCard || __instance.SpecialCard != null)
            return;

        __instance.RewardType = RewardType.RemoveCard;
        __instance.OptionCount = DetoxDeckUpgradeReward.SaveMarker;
    }
}
