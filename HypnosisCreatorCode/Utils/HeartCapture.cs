using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

public static class HeartCapture
{
    private static readonly List<(Player Player, string MonsterId)> Pending = [];

    /// <summary>
    /// 戦闘終了時に解決する捕獲を予約する。
    /// 付与対象が死亡してパワーが消えても心臓を落とせるようにする。
    /// </summary>
    public static void QueueCapture(Player player, string monsterIdEntry)
    {
        if (string.IsNullOrWhiteSpace(monsterIdEntry)) return;
        Pending.Add((player, monsterIdEntry));
        MainFile.Logger.Info($"Heart capture queued for {monsterIdEntry}");
    }

    public static void QueueCapture(Player player, Creature target)
    {
        if (!target.IsMonster) return;
        var monsterId = target.Monster?.Id.Entry ?? target.ModelId.Entry;
        QueueCapture(player, monsterId);
    }

    /// <summary>予約済み捕獲をすべて解決する（戦闘終了フックから呼ぶ）。</summary>
    public static async Task FlushPending()
    {
        if (Pending.Count == 0) return;

        var batch = Pending.ToList();
        Pending.Clear();
        foreach (var (player, monsterId) in batch)
            await TryCapture(player, monsterId);
    }

    /// <summary>
    /// 戦闘報酬画面に「追加のレリック報酬」として敵固有心臓を載せる。
    /// エリート等の通常報酬と並ぶ（競合して消えない）。部屋が取れない場合は即時 Obtain にフォールバック。
    /// </summary>
    public static void TryAddExtraRelicReward(Player player, Creature slain)
    {
        if (!slain.IsMonster) return;
        var monsterId = slain.Monster?.Id.Entry ?? slain.ModelId.Entry;
        TryAddExtraRelicReward(player, monsterId);
    }

    public static void TryAddExtraRelicReward(Player player, string monsterIdEntry)
    {
        if (string.IsNullOrWhiteSpace(monsterIdEntry)) return;

        var relic = CreateHeartRelic(monsterIdEntry);
        if (relic == null)
        {
            MainFile.Logger.Warn($"Failed to create heart relic for {monsterIdEntry}");
            return;
        }

        if (player.RunState.CurrentRoom is CombatRoom room)
        {
            room.AddExtraReward(player, new RelicReward(relic, player));
            MainFile.Logger.Info($"Extra relic reward added: {relic.Id.Entry} from {monsterIdEntry}");
            return;
        }

        MainFile.Logger.Info($"No CombatRoom for extra reward; fallback Obtain for {monsterIdEntry}");
        _ = TryCapture(player, monsterIdEntry);
    }

    /// <summary>リーサル時などに敵固有心臓を即時付与。未登録モンスターは StolenHeart にフォールバック。</summary>
    public static async Task TryCapture(Player player, Creature slain)
    {
        if (!slain.IsMonster) return;
        var monsterId = slain.Monster?.Id.Entry ?? slain.ModelId.Entry;
        await TryCapture(player, monsterId);
    }

    public static async Task TryCapture(Player player, string monsterIdEntry)
    {
        if (string.IsNullOrWhiteSpace(monsterIdEntry)) return;

        MainFile.Logger.Info($"Heart capture from {monsterIdEntry}");

        var relic = CreateHeartRelic(monsterIdEntry);
        if (relic == null)
        {
            await RelicCmd.Obtain<StolenHeart>(player);
            return;
        }

        await RelicCmd.Obtain(relic, player);
    }

    private static RelicModel? CreateHeartRelic(string monsterIdEntry)
    {
        var heartType = HeartRegistry.ResolveHeartType(monsterIdEntry) ?? typeof(StolenHeart);
        var canonical = ModelDb.AllRelics.FirstOrDefault(r => r.GetType() == heartType);
        if (canonical != null)
            return canonical.ToMutable();

        if (Activator.CreateInstance(heartType) is RelicModel created)
            return created;

        return null;
    }
}
