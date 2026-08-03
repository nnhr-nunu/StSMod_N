using System.Runtime.CompilerServices;
using HypnosisCreator.HypnosisCreatorCode;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 布教欲求の戦闘終了ゴールドをプレイヤー単位で溜める。
/// SpireField は保持に失敗する事例があるため <see cref="ConditionalWeakTable{TKey,TValue}"/> を使う。
/// 清算時は本家 Royalties と同様、報酬画面の追加 GoldReward として出す。
/// 累計は <see cref="ProselytizeGoldPower"/> のアイコン数字で表示する。
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

    /// <summary>カードプレイ中は choiceContext を渡して await する（GetResult 禁止）。</summary>
    public static async Task AddGoldAsync(PlayerChoiceContext choiceContext, Player player, decimal amount)
    {
        AddGold(player, amount);
        await SyncGoldPowerAsync(choiceContext, player);
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
        ScheduleSyncGoldPower(player);
        return amount;
    }

    /// <summary>互換: Creature 経由。</summary>
    public static decimal TakeGold(Creature playerCreature) =>
        playerCreature?.Player is { } player ? TakeGold(player) : 0M;

    /// <summary>同期コンテキスト外（戦闘終了清算など）からの反映。GetResult は使わない。</summary>
    public static void ScheduleSyncGoldPower(Player player) =>
        _ = SyncGoldPowerAsync(new ThrowingPlayerChoiceContext(), player);

    /// <summary>累計ゴールドをプレイヤーバフアイコンの数字へ反映する。</summary>
    public static Task SyncGoldPowerAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var creature = player.Creature;
        if (creature == null) return Task.CompletedTask;

        return SyncGoldPowerCoreAsync(choiceContext, creature, player);
    }

    private static async Task SyncGoldPowerCoreAsync(
        PlayerChoiceContext choiceContext,
        Creature creature,
        Player player)
    {
        var total = (int)Math.Max(0M, PeekGold(player));

        try
        {
            if (total <= 0)
            {
                var existing = creature.GetPower<ProselytizeGoldPower>();
                if (existing != null)
                    await PowerCmd.Remove(existing);
                return;
            }

            var power = creature.GetPower<ProselytizeGoldPower>();
            if (power == null)
            {
                await PowerCmd.Apply<ProselytizeGoldPower>(
                    choiceContext, creature, total, creature, null, silent: true);
                return;
            }

            var delta = total - (int)power.Amount;
            if (delta == 0) return;

            await PowerCmd.ModifyAmount(choiceContext, power, delta, creature, null, silent: true);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Proselytize gold power sync failed: {ex.Message}");
        }
    }
}
