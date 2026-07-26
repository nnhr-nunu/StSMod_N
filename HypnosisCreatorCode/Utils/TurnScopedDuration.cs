namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>「このターン」系パワーの残りターン数を加算・消費する。</summary>
internal static class TurnScopedDuration
{
    public static int AddStack(int current) => Math.Max(0, current) + 1;

    public static bool Consume(ref int remaining) => --remaining <= 0;
}
