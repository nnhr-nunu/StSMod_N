using BaseLib.Config;
using Godot;

namespace HypnosisCreator.HypnosisCreatorCode.Config;

/// <summary>
/// Mod設定（Settings → Mod Settings → Hypno Creator）。
/// スライダーを動かすと VisualTuner 経由で即反映され、閉じると自動保存される。
/// </summary>
public sealed class HypnosisCreatorConfig : SimpleModConfig
{
    public HypnosisCreatorConfig()
    {
        ConfigChanged += (_, _) => VisualTuner.ApplyAll();
        OnConfigReloaded += VisualTuner.ApplyAll;
    }

    // --- クロマキー（立ち絵 GB 透過） ---

    [ConfigSection("ChromaKey")]
    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaSimilarity { get; set; } = 0.22;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaSmoothness { get; set; } = 0.08;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaSpill { get; set; } = 0.25;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaKeyR { get; set; } = 0.03;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaKeyG { get; set; } = 0.63;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaKeyB { get; set; } = 0.21;

    // --- キャラ選択 1枚背景 ---

    [ConfigSection("SelectBackground")]
    [ConfigSlider(-0.5, 0.5, 0.01, Format = "{0:0.00}")]
    public static double SelectBgOffsetX { get; set; }

    [ConfigSlider(-0.5, 0.5, 0.01, Format = "{0:0.00}")]
    public static double SelectBgOffsetY { get; set; }

    [ConfigSlider(0.5, 3.0, 0.01, Format = "{0:0.00}x")]
    public static double SelectBgZoom { get; set; } = 1.0;

    // --- カード絵（全 Hypno Creator カード共通） ---

    [ConfigSection("CardArt")]
    [ConfigSlider(-0.5, 0.5, 0.01, Format = "{0:0.00}")]
    public static double CardOffsetX { get; set; }

    [ConfigSlider(-0.5, 0.5, 0.01, Format = "{0:0.00}")]
    public static double CardOffsetY { get; set; }

    [ConfigSlider(0.5, 3.0, 0.01, Format = "{0:0.00}x")]
    public static double CardZoom { get; set; } = 1.0;

    public static Color GetChromaKeyColor() =>
        new((float)ChromaKeyR, (float)ChromaKeyG, (float)ChromaKeyB, 1f);
}
