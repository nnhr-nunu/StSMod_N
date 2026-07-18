using System.Text.Json;

namespace HypnosisCreator.HypnosisCreatorCode.Config;

/// <summary>
/// カード絵切り抜きの個別設定（JSON 文字列として ModConfig に保存）。
/// </summary>
public static class CardCropStore
{
    public const string DefaultKey = "_default";

    public readonly record struct Crop(double OffsetX, double OffsetY, double Zoom)
    {
        public static Crop FromConfigSliders() =>
            new(HypnosisCreatorConfig.CardOffsetX, HypnosisCreatorConfig.CardOffsetY, HypnosisCreatorConfig.CardZoom);

        public static Crop Defaults => new(0, 0, 1);
    }

    public static string NormalizeKey(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return DefaultKey;
        var key = raw.Trim().ToLowerInvariant()
            .Replace('-', '_')
            .Replace(' ', '_');
        // HYPNOSISCREATOR-COOKING_HYPNOSIS → cooking_hypnosis
        var idx = key.LastIndexOf(':');
        if (idx >= 0) key = key[(idx + 1)..];
        idx = key.LastIndexOf('-');
        if (idx >= 0 && key.Contains("hypnosiscreator", StringComparison.Ordinal))
            key = key[(idx + 1)..];
        return string.IsNullOrEmpty(key) ? DefaultKey : key;
    }

    public static string KeyFromTexturePath(string path)
    {
        var file = Path.GetFileNameWithoutExtension(path);
        return NormalizeKey(file);
    }

    public static Dictionary<string, Crop> LoadAll()
    {
        var json = HypnosisCreatorConfig.CardCropOverridesJson;
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, Crop>(StringComparer.OrdinalIgnoreCase);

        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, CropDto>>(json);
            if (raw == null) return new Dictionary<string, Crop>(StringComparer.OrdinalIgnoreCase);

            var result = new Dictionary<string, Crop>(StringComparer.OrdinalIgnoreCase);
            foreach (var (k, v) in raw)
                result[NormalizeKey(k)] = new Crop(v.X, v.Y, v.Z <= 0 ? 1 : v.Z);
            return result;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"CardCropStore.LoadAll failed: {ex.Message}");
            return new Dictionary<string, Crop>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public static void SaveAll(Dictionary<string, Crop> map)
    {
        var dto = new Dictionary<string, CropDto>(StringComparer.OrdinalIgnoreCase);
        foreach (var (k, v) in map)
            dto[NormalizeKey(k)] = new CropDto { X = v.OffsetX, Y = v.OffsetY, Z = v.Zoom };

        HypnosisCreatorConfig.CardCropOverridesJson =
            JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = false });
    }

    public static Crop Get(string key, Dictionary<string, Crop>? map = null)
    {
        map ??= LoadAll();
        key = NormalizeKey(key);
        if (map.TryGetValue(key, out var crop)) return crop;
        if (map.TryGetValue(DefaultKey, out var fallback)) return fallback;
        return Crop.Defaults;
    }

    public static void UpsertFromSliders(string key)
    {
        var map = LoadAll();
        map[NormalizeKey(key)] = Crop.FromConfigSliders();
        SaveAll(map);
    }

    public static void ApplyCropToSliders(string key)
    {
        var crop = Get(key);
        HypnosisCreatorConfig.CardOffsetX = crop.OffsetX;
        HypnosisCreatorConfig.CardOffsetY = crop.OffsetY;
        HypnosisCreatorConfig.CardZoom = crop.Zoom;
    }

    public static IEnumerable<string> KnownCardKeys()
    {
        yield return DefaultKey;
        // 実装済みカード（ファイル名規則）。網羅目的ではなく開発時のサンプル表示用。
        foreach (var name in new[]
                 {
                     "hc_defend", "pleasure_cycle", "kick", "drug_hypnosis", "zero_out",
                     "soft_technique", "whisper", "harmony", "mirroring", "discipline",
                     "prefinger", "polynesian_hypnosis", "know_it_all", "total_control"
                 })
            yield return name;
    }

    private sealed class CropDto
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; } = 1;
    }
}
