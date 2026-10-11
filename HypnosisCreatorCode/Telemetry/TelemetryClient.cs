using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves;

namespace HypnosisCreator.HypnosisCreatorCode.Telemetry;

internal enum TelemetrySendStatus
{
    Accepted,
    /// <summary>サーバーが内容を不正と判断した（400/413）。再送しても通らない。</summary>
    Rejected,
    RateLimited,
    /// <summary>通信できない・サーバー側の失敗。時間をおけば通る見込みがある。</summary>
    Unavailable,
}

/// <summary>
/// フィードバックと匿名プレイデータを自前の Cloudflare Worker（リポジトリの server/）へ送る。
/// MOD に入るのはこの URL だけで、通知先（Discord）やメールアドレスは Worker 側の secret にしかない。
/// </summary>
internal static partial class TelemetryClient
{
    private const string BaseUrl = "https://stsmod-n.nnhr.workers.dev/";

    public static readonly Uri FeedbackEndpoint = new(BaseUrl + "v1/feedback");
    public static readonly Uri RunsEndpoint = new(BaseUrl + "v1/runs");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private static readonly HttpClient Http = new(new HttpClientHandler
    {
        AllowAutoRedirect = false,
        UseCookies = false,
    })
    {
        Timeout = TimeSpan.FromSeconds(15),
    };

    private static string? _modVersion;

    /// <summary>manifest の version（例: v0.1.1）。読めなければ unknown。</summary>
    public static string ModVersion => _modVersion ??= SafeToken(
        ModManager.Mods.FirstOrDefault(m => m.manifest?.id == MainFile.ModId)?.manifest?.version);

    /// <summary>ゲームの表示言語（jpn / eng などの3文字）。読めなければ unknown。</summary>
    public static string Language
    {
        get
        {
            try
            {
                var language = SaveManager.Instance.SettingsSave?.Language?.ToLowerInvariant();
                return language != null && LanguageCode().IsMatch(language) ? language : "unknown";
            }
            catch
            {
                return "unknown";
            }
        }
    }

    public static string NewReportId() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    /// <summary>同じランの再送を1件にまとめるための ID（個人を特定できる値は混ぜない）。</summary>
    public static string StableReportId(string source) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))).ToLowerInvariant();

    /// <summary>Worker 側の検証（英数字と ._+- のみ）に合わない値は unknown にする。</summary>
    public static string SafeToken(string? value) =>
        value != null && Token().IsMatch(value) ? value : "unknown";

    /// <summary>モデル ID（CARD.XXX など）が Worker 側の検証に合うときだけ返す。</summary>
    public static bool IsSafeModelId(string? value) => value != null && ModelIdPattern().IsMatch(value);

    public static async Task<TelemetrySendStatus> PostAsync<T>(Uri endpoint, T body)
    {
        try
        {
            using var content = new ByteArrayContent(JsonSerializer.SerializeToUtf8Bytes(body, JsonOptions));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using var response = await Http.PostAsync(endpoint, content).ConfigureAwait(false);
            return response.StatusCode switch
            {
                HttpStatusCode.OK => TelemetrySendStatus.Accepted,
                HttpStatusCode.TooManyRequests => TelemetrySendStatus.RateLimited,
                HttpStatusCode.BadRequest or HttpStatusCode.RequestEntityTooLarge => TelemetrySendStatus.Rejected,
                _ => TelemetrySendStatus.Unavailable,
            };
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Telemetry POST {endpoint.AbsolutePath} failed: {ex.Message}");
            return TelemetrySendStatus.Unavailable;
        }
    }

    [GeneratedRegex("^[a-z]{3}$")]
    private static partial Regex LanguageCode();

    [GeneratedRegex(@"^[A-Za-z0-9_.+\-]{1,64}$")]
    private static partial Regex Token();

    [GeneratedRegex(@"^[A-Za-z0-9_.\-]{1,128}$")]
    private static partial Regex ModelIdPattern();
}
