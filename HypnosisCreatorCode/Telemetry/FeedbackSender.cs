namespace HypnosisCreator.HypnosisCreatorCode.Telemetry;

internal enum FeedbackSendResult
{
    Sent,
    Busy,
    RateLimited,
    Invalid,
    Failed,
}

internal sealed record FeedbackReport(
    int SchemaVersion,
    string ReportId,
    string ModVersion,
    string Language,
    string Message);

/// <summary>設定画面の「フィードバックを送る」。本文・MOD のバージョン・表示言語だけを送る。</summary>
internal static class FeedbackSender
{
    public const int MaxCharacters = 8000;

    private static int _inFlight;

    public static async Task<FeedbackSendResult> SendAsync(string message)
    {
        message = message.Trim();
        if (message.Length is 0 or > MaxCharacters || message.Any(IsForbiddenControl))
            return FeedbackSendResult.Invalid;
        if (Interlocked.CompareExchange(ref _inFlight, 1, 0) != 0)
            return FeedbackSendResult.Busy;

        try
        {
            var report = new FeedbackReport(1, TelemetryClient.NewReportId(), TelemetryClient.ModVersion,
                TelemetryClient.Language, message);
            var status = await Task.Run(() => TelemetryClient.PostAsync(TelemetryClient.FeedbackEndpoint, report));
            return status switch
            {
                TelemetrySendStatus.Accepted => FeedbackSendResult.Sent,
                TelemetrySendStatus.RateLimited => FeedbackSendResult.RateLimited,
                TelemetrySendStatus.Rejected => FeedbackSendResult.Invalid,
                _ => FeedbackSendResult.Failed,
            };
        }
        finally
        {
            Volatile.Write(ref _inFlight, 0);
        }
    }

    // タブ・改行以外の制御文字は Worker 側でも弾く
    private static bool IsForbiddenControl(char c) => char.IsControl(c) && c is not ('\t' or '\n' or '\r');
}
