using HypnosisCreator.HypnosisCreatorCode.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 認知シャッフル期限切れ。カードプレイ中の PowerCmd 競合を避けるため、
/// トランス解除・敵死亡時は <see cref="Request"/> し AfterCardPlayed 後に実行する。
/// </summary>
public static class CognitiveShuffleExpire
{
    private static readonly HashSet<CognitiveShufflePower> Pending = [];
    private static readonly object Lock = new();

    public static void Request(CognitiveShufflePower power)
    {
        if (power.Owner == null) return;
        lock (Lock)
            Pending.Add(power);
    }

    public static bool HasPending
    {
        get
        {
            lock (Lock)
                return Pending.Count > 0;
        }
    }

    public static async Task RunPendingAsync()
    {
        List<CognitiveShufflePower> batch;
        lock (Lock)
        {
            if (Pending.Count == 0) return;
            batch = Pending.ToList();
            Pending.Clear();
        }

        foreach (var power in batch)
            await power.ExpireFromSchedulerAsync();
    }
}
