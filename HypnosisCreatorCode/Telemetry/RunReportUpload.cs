using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace HypnosisCreator.HypnosisCreatorCode.Telemetry;

/// <summary>
/// 本家がラン履歴を保存した直後に、匿名プレイデータを送る（本家の「データ送信」が ON のときだけ）。
/// 履歴を読むだけで、ランの状態は変えない（共闘の同期に影響しない）。
/// </summary>
[HarmonyPatch(typeof(RunHistorySaveManager), nameof(RunHistorySaveManager.SaveHistory))]
internal static class RunHistorySavedPatch
{
    [HarmonyPostfix]
    private static void Postfix(RunHistory history)
    {
        try
        {
            if (!UploadDataSetting.IsEnabled)
                return;
            var report = RunReportBuilder.TryCreate(history);
            if (report != null)
                RunReportUploader.Enqueue(report);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Run report capture failed: {ex.Message}");
        }
    }
}

internal static class RunReportUploader
{
    // オフライン等で届かなかったときの再送間隔。全部失敗したら諦める（ゲームを閉じても消える）
    private static readonly TimeSpan[] RetryDelays =
        [TimeSpan.FromSeconds(30), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(10)];

    public static void Enqueue(RunReport report) => _ = Task.Run(() => SendAsync(report));

    private static async Task SendAsync(RunReport report)
    {
        for (var attempt = 0; ; attempt++)
        {
            var status = await TelemetryClient.PostAsync(TelemetryClient.RunsEndpoint, report).ConfigureAwait(false);
            switch (status)
            {
                case TelemetrySendStatus.Accepted:
                    MainFile.Logger.Info("Run report sent.");
                    return;
                case TelemetrySendStatus.Rejected:
                    MainFile.Logger.Warn("Run report was rejected by the server (schema mismatch?).");
                    return;
            }

            if (attempt >= RetryDelays.Length)
            {
                MainFile.Logger.Warn($"Run report dropped after {attempt + 1} attempts ({status}).");
                return;
            }

            await Task.Delay(RetryDelays[attempt]).ConfigureAwait(false);
            // 待っている間に送信を OFF にされたら送らない
            if (!UploadDataSetting.IsEnabled)
                return;
        }
    }
}
