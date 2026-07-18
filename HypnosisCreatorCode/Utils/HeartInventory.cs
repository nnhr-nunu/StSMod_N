using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>心臓レリックの数え上げ・再使用。mechanics-lock / CSV No.68,85,86。</summary>
public static class HeartInventory
{
    public static bool IsHeartRelic(RelicModel relic) =>
        relic is StolenHeart or EnemyHeartRelic;

    public static int CountHearts(Player player) =>
        player.Relics.Count(IsHeartRelic);

    public static IEnumerable<EnemyHeartRelic> OwnedEnemyHearts(Player player) =>
        player.Relics.OfType<EnemyHeartRelic>();

    /// <summary>No.86: この戦闘中、希少な心臓を再使用可能にする。</summary>
    public static void RefreshAllForCombat(Player player)
    {
        foreach (var heart in OwnedEnemyHearts(player))
            heart.RefreshForCombat();
    }

    /// <summary>No.86 UG: 戦闘外でも再使用可能にする。</summary>
    public static void RefreshAllPermanently(Player player)
    {
        foreach (var heart in OwnedEnemyHearts(player))
            heart.RefreshPermanently();
    }
}
