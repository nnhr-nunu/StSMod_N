namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 本家は同一バージョンならローカル mods/ を Workshop より優先する。
/// 読み込み元判定（godot.log とマルチ「ホストに足りません」の切り分け用）。
/// </summary>
internal static class ModLoadSource
{
    public static bool IsWorkshopPath(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        return Normalize(path).Contains("/workshop/content/", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsLocalModsPath(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        var normalized = Normalize(path);
        return normalized.Contains("/mods/", StringComparison.OrdinalIgnoreCase) && !IsWorkshopPath(normalized);
    }

    public static string Normalize(string path) => path.Replace('\\', '/');
}
