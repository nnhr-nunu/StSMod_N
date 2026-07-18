using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>催眠導入 — 同名カードを同一対象へプレイした回数を記録し、ドロー枚数を減衰させる。</summary>
public static class HypnosisIntroDrawTracker
{
    private static readonly NotNullSpireField<Player, Dictionary<(int TargetId, string CardKey), int>> Field =
        new(() => new Dictionary<(int, string), int>());

    public static int GetPriorPlayCount(Player player, Creature target, string cardKey) =>
        Field.Get(player).TryGetValue((target.GetHashCode(), cardKey), out var count) ? count : 0;

    public static void RecordPlay(Player player, Creature target, string cardKey)
    {
        var key = (target.GetHashCode(), cardKey);
        var dict = Field.Get(player);
        dict[key] = dict.TryGetValue(key, out var count) ? count + 1 : 1;
    }

    public static void Reset(Player player) => Field.Get(player).Clear();
}
