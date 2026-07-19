using System.Runtime.CompilerServices;
using HypnosisCreator.HypnosisCreatorCode;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 布教欲求の戦闘終了ゴールドをプレイヤー単位で溜める。
/// SpireField は保持に失敗する事例があるため <see cref="ConditionalWeakTable{TKey,TValue}"/> を使う。
/// 清算時は本家 Royalties と同様、報酬画面の追加 GoldReward として出す。
/// </summary>
public static class ProselytizeRewards
{
    private sealed class GoldState
    {
        public decimal Amount;
    }

    private static readonly ConditionalWeakTable<Player, GoldState> Table = new();

    public static void AddGold(Player player, decimal amount)
    {
        if (player == null || amount <= 0M) return;
        Table.GetValue(player, static _ => new GoldState()).Amount += amount;
        MainFile.Logger.Info(
            $"Proselytize gold pending: +{amount} (total {PeekGold(player)}) for {player.Character?.Id.Entry}");
    }

    /// <summary>互換: Creature から Player を解決して加算。</summary>
    public static void AddGold(Creature playerCreature, decimal amount)
    {
        if (playerCreature?.Player is not { } player) return;
        AddGold(player, amount);
    }

    public static decimal PeekGold(Player player)
    {
        if (player == null) return 0M;
        return Table.TryGetValue(player, out var state) ? state.Amount : 0M;
    }

    public static decimal TakeGold(Player player)
    {
        if (player == null) return 0M;
        if (!Table.TryGetValue(player, out var state)) return 0M;
        var amount = state.Amount;
        state.Amount = 0M;
        return amount;
    }

    /// <summary>互換: Creature 経由。</summary>
    public static decimal TakeGold(Creature playerCreature) =>
        playerCreature?.Player is { } player ? TakeGold(player) : 0M;
}
