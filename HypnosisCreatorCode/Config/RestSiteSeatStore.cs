using System.Text.Json;
using Godot;

namespace HypnosisCreator.HypnosisCreatorCode.Config;

/// <summary>篝火立ち絵の席別位置オフセット（JSON で ModConfig に保存）。</summary>
public static class RestSiteSeatStore
{
    public const int SeatCount = 4;

    public readonly record struct Offset(double X, double Y)
    {
        public static Offset FromSliders() =>
            new(HypnosisCreatorConfig.RestSiteOffsetX, HypnosisCreatorConfig.RestSiteOffsetY);

        public Vector2 ToVector2() => new((float)X, (float)Y);
    }

    public static int ClampSeat(int seat) => Math.Clamp(seat, 0, SeatCount - 1);

    public static Offset GetDefault(int seat) => seat switch
    {
        0 => new Offset(6, 77),
        1 => new Offset(-38, 172),
        2 => new Offset(-200, 103),
        _ => new Offset(0, 0)
    };

    public static Dictionary<string, Offset> LoadAll()
    {
        var json = HypnosisCreatorConfig.RestSiteSeatOffsetsJson;
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, Offset>(StringComparer.Ordinal);

        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, OffsetDto>>(json);
            if (raw == null)
                return new Dictionary<string, Offset>(StringComparer.Ordinal);

            var result = new Dictionary<string, Offset>(StringComparer.Ordinal);
            foreach (var (k, v) in raw)
                result[k] = new Offset(v.X, v.Y);
            return result;
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"RestSiteSeatStore.LoadAll failed: {ex.Message}");
            return new Dictionary<string, Offset>(StringComparer.Ordinal);
        }
    }

    public static void SaveAll(Dictionary<string, Offset> map)
    {
        var dto = new Dictionary<string, OffsetDto>(StringComparer.Ordinal);
        foreach (var (k, v) in map)
            dto[k] = new OffsetDto { X = v.X, Y = v.Y };

        HypnosisCreatorConfig.RestSiteSeatOffsetsJson =
            JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = false });
    }

    public static Offset Get(int seat, Dictionary<string, Offset>? map = null)
    {
        seat = ClampSeat(seat);
        map ??= LoadAll();
        var key = seat.ToString();
        if (map.TryGetValue(key, out var offset))
            return offset;
        return GetDefault(seat);
    }

    public static void UpsertFromSliders(int seat)
    {
        var map = LoadAll();
        map[ClampSeat(seat).ToString()] = Offset.FromSliders();
        SaveAll(map);
    }

    public static void RemoveSeat(int seat)
    {
        var key = ClampSeat(seat).ToString();
        var map = LoadAll();
        if (!map.Remove(key))
            return;
        SaveAll(map);
    }

    public static void ApplyToSliders(int seat)
    {
        var offset = Get(seat);
        HypnosisCreatorConfig.RestSiteOffsetX = offset.X;
        HypnosisCreatorConfig.RestSiteOffsetY = offset.Y;
    }

    public static void ResetAllDefaults()
    {
        HypnosisCreatorConfig.RestSiteSeatOffsetsJson = "{}";
        HypnosisCreatorConfig.RestSitePreviewSeat = 0;
        HypnosisCreatorConfig.RestSiteUsePreviewLayout = 0;
        ApplyToSliders(0);
    }

    private sealed class OffsetDto
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
