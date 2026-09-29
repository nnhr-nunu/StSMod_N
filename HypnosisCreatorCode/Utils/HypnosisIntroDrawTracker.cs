using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>催眠導入 — 同名カードを同一対象へプレイした回数を記録し、ドロー枚数を減衰させる。</summary>
public static class HypnosisIntroDrawTracker
{
    // 対象は Creature の参照で引く。GetHashCode（プロセスごとに違う値）だと、前の戦闘の記録と
    // 片方の端末でだけ衝突してドロー枚数が変わり、共闘で同期がずれうる。戦闘開始時に Reset する。
    private static readonly NotNullSpireField<Player, Dictionary<(Creature Target, string CardKey), int>> Field =
        new(() => new Dictionary<(Creature, string), int>());

    public static int GetPriorPlayCount(Player player, Creature target, string cardKey) =>
        Field.Get(player).TryGetValue((target, cardKey), out var count) ? count : 0;

    public static void RecordPlay(Player player, Creature target, string cardKey)
    {
        var key = (target, cardKey);
        var dict = Field.Get(player);
        dict[key] = dict.TryGetValue(key, out var count) ? count + 1 : 1;
    }

    public static void Reset(Player player) => Field.Get(player).Clear();
}
