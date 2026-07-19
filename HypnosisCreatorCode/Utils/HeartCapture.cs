using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

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

        var heartType = HeartRegistry.ResolveHeartType(monsterIdEntry);
        if (heartType != null)
        {
            await ObtainHeart(player, heartType);
            return;
        }

        MainFile.Logger.Info($"No enemy heart mapped for {monsterIdEntry}; fallback StolenHeart");
        await RelicCmd.Obtain<StolenHeart>(player);
    }

    private static async Task ObtainHeart(Player player, Type heartType)
    {
        var method = typeof(RelicCmd).GetMethods()
            .First(m => m.Name == nameof(RelicCmd.Obtain) && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
        var task = (Task)method.MakeGenericMethod(heartType).Invoke(null, [player])!;
        await task;
    }
}
