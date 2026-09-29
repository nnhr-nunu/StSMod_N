using HypnosisCreator.HypnosisCreatorCode.Relics;
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

    /// <summary>
    /// 所持済みの心臓タイプか（同名レリックの再ドロップ抑止）。
    /// 戦闘終了で消える一時コピー（自己暗示など）は数えない（数えると本物の報酬が落ちず、戦闘後には何も残らない）。
    /// <see cref="StolenHeart"/> は重ねて持てる仕様なので常に「未所持」扱い。
    /// </summary>
    public static bool OwnsHeartType(Player player, Type heartType)
    {
        if (heartType == typeof(StolenHeart)) return false;
        return player.Relics.Any(r =>
            r.GetType() == heartType && r is not HypnosisCreatorRelic { RemoveAtCombatEnd: true });
    }

    /// <summary>No.86: 使用済み希少心臓だけ、この戦闘中再使用可能にする。</summary>
    public static void RefreshAllForCombat(Player player)
    {
        foreach (var heart in OwnedEnemyHearts(player))
            heart.RefreshForCombat();
    }

    /// <summary>No.86 UG: 使用済み希少心臓の層内1回制限をリセット（戦闘後も維持）。</summary>
    public static void RefreshAllAfterCravingUpgraded(Player player)
    {
        foreach (var heart in OwnedEnemyHearts(player))
            heart.RefreshAfterCravingUpgraded();
    }

    /// <summary>層クリア後: 希少心臓を当該層で再使用可能にする。</summary>
    public static void RefreshAllForNewAct(Player player)
    {
        foreach (var heart in OwnedEnemyHearts(player))
            heart.RefreshForNewAct();
    }
}
