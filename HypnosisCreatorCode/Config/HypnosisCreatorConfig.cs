using BaseLib.Config;
using BaseLib.Config.UI;
using Godot;

namespace HypnosisCreator.HypnosisCreatorCode.Config;

/// <summary>
/// Mod設定（Settings → Mod Settings → Hypno Creator）。
/// スライダーを動かすと VisualTuner 経由で即反映され、閉じると自動保存される。
/// </summary>
public sealed class HypnosisCreatorConfig : SimpleModConfig
{
    private static bool _syncingCardTarget;
    private static string _lastCardTarget = CardCropStore.DefaultKey;

    public HypnosisCreatorConfig()
    {
        ConfigChanged += (_, _) => OnAnyConfigChanged();
        OnConfigReloaded += OnAnyConfigChanged;
    }

    // --- クロマキー（立ち絵 GB 透過） ---

    [ConfigSection("ChromaKey")]
    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaSimilarity { get; set; } = HypnosisCreatorConfigDefaults.ChromaSimilarity;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaSmoothness { get; set; } = HypnosisCreatorConfigDefaults.ChromaSmoothness;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaSpill { get; set; } = HypnosisCreatorConfigDefaults.ChromaSpill;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaKeyR { get; set; } = HypnosisCreatorConfigDefaults.ChromaKeyR;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaKeyG { get; set; } = HypnosisCreatorConfigDefaults.ChromaKeyG;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaKeyB { get; set; } = HypnosisCreatorConfigDefaults.ChromaKeyB;

    [ConfigButton("ResetChromaDefaults")]
    public static void OnResetChromaDefaults(ModConfig cfg, NConfigOptionRow row)
    {
        _ = row;
        ChromaSimilarity = HypnosisCreatorConfigDefaults.ChromaSimilarity;
        ChromaSmoothness = HypnosisCreatorConfigDefaults.ChromaSmoothness;
        ChromaSpill = HypnosisCreatorConfigDefaults.ChromaSpill;
        ChromaKeyR = HypnosisCreatorConfigDefaults.ChromaKeyR;
        ChromaKeyG = HypnosisCreatorConfigDefaults.ChromaKeyG;
        ChromaKeyB = HypnosisCreatorConfigDefaults.ChromaKeyB;
        cfg.Changed();
    }

    // --- ウォーターマーク隠し（立ち絵右下ロゴ。PNGは改変せずシェーダーで透過） ---

    [ConfigSection("Watermark")]
    [ConfigSlider(0.0, 0.25, 0.005, Format = "{0:0.000}")]
    public static double WatermarkCropBottom { get; set; } = HypnosisCreatorConfigDefaults.WatermarkCropBottom;

    [ConfigSlider(0.0, 0.8, 0.01, Format = "{0:0.00}")]
    public static double WatermarkCropSide { get; set; } = HypnosisCreatorConfigDefaults.WatermarkCropSide;

    /// <summary>1＝既定。上げると足元の白文字ロゴ検出を強める（靴欠けに注意）。</summary>
    [ConfigSlider(0.0, 2.0, 0.05, Format = "{0:0.00}")]
    public static double WatermarkLogoStrength { get; set; } = HypnosisCreatorConfigDefaults.WatermarkLogoStrength;

    [ConfigButton("ResetWatermarkDefaults")]
    public static void OnResetWatermarkDefaults(ModConfig cfg, NConfigOptionRow row)
    {
        _ = row;
        WatermarkCropBottom = HypnosisCreatorConfigDefaults.WatermarkCropBottom;
        WatermarkCropSide = HypnosisCreatorConfigDefaults.WatermarkCropSide;
        WatermarkLogoStrength = HypnosisCreatorConfigDefaults.WatermarkLogoStrength;
        cfg.Changed();
    }

    // --- キャラ選択 1枚背景（単位: px。Yプラス＝画面下へ） ---

    [ConfigSection("SelectBackground")]
    [ConfigSlider(-400, 400, 1, Format = "{0:0}px")]
    public static double SelectBgOffsetX { get; set; } = HypnosisCreatorConfigDefaults.SelectBgOffsetX;

    [ConfigSlider(-400, 400, 1, Format = "{0:0}px")]
    public static double SelectBgOffsetY { get; set; } = HypnosisCreatorConfigDefaults.SelectBgOffsetY;

    [ConfigSlider(0.5, 3.0, 0.01, Format = "{0:0.00}x")]
    public static double SelectBgZoom { get; set; } = HypnosisCreatorConfigDefaults.SelectBgZoom;

    [ConfigButton("ResetSelectBackgroundDefaults")]
    public static void OnResetSelectBackgroundDefaults(ModConfig cfg, NConfigOptionRow row)
    {
        _ = row;
        SelectBgOffsetX = HypnosisCreatorConfigDefaults.SelectBgOffsetX;
        SelectBgOffsetY = HypnosisCreatorConfigDefaults.SelectBgOffsetY;
        SelectBgZoom = HypnosisCreatorConfigDefaults.SelectBgZoom;
        cfg.Changed();
    }

    // --- カード絵（個別: CardTargetId で対象を指定） ---

    [ConfigSection("CardArt")]
    [ConfigTextInput(@"[A-Za-z0-9_\-]*")]
    public static string CardTargetId { get; set; } = CardCropStore.DefaultKey;

    [ConfigSlider(-0.5, 0.5, 0.01, Format = "{0:0.00}")]
    public static double CardOffsetX { get; set; } = HypnosisCreatorConfigDefaults.CardOffsetX;

    [ConfigSlider(-0.5, 0.5, 0.01, Format = "{0:0.00}")]
    public static double CardOffsetY { get; set; } = HypnosisCreatorConfigDefaults.CardOffsetY;

    [ConfigSlider(0.5, 3.0, 0.01, Format = "{0:0.00}x")]
    public static double CardZoom { get; set; } = HypnosisCreatorConfigDefaults.CardZoom;

    /// <summary>カードごとの切り抜き保存領域（UI非表示）。</summary>
    [ConfigHideInUI]
    public static string CardCropOverridesJson { get; set; } = "{}";

    [ConfigButton("ResetCardArtDefaults")]
    public static void OnResetCardArtDefaults(ModConfig cfg, NConfigOptionRow row)
    {
        _ = row;
        var target = CardCropStore.NormalizeKey(CardTargetId);
        CardOffsetX = HypnosisCreatorConfigDefaults.CardOffsetX;
        CardOffsetY = HypnosisCreatorConfigDefaults.CardOffsetY;
        CardZoom = HypnosisCreatorConfigDefaults.CardZoom;
        CardCropStore.RemoveKey(target);
        cfg.Changed();
    }

    [ConfigButton("ListCardIds")]
    public static void OnListCardIds(ModConfig cfg, NConfigOptionRow row)
    {
        _ = cfg;
        _ = row;
        var ids = string.Join(", ", CardCropStore.KnownCardKeys());
        MainFile.Logger.Info($"Card crop target ids: {ids}");
        MainFile.Logger.Info("Use _default for all cards without a specific override. Example: cooking_hypnosis");
    }

    public static Color GetChromaKeyColor() =>
        new((float)ChromaKeyR, (float)ChromaKeyG, (float)ChromaKeyB, 1f);

    private static void OnAnyConfigChanged()
    {
        if (_syncingCardTarget)
        {
            VisualTuner.ApplyAll();
            return;
        }

        var target = CardCropStore.NormalizeKey(CardTargetId);
        if (target != _lastCardTarget)
        {
            // 対象カード切替 → 保存済み値をスライダーへ読み込み
            _syncingCardTarget = true;
            try
            {
                _lastCardTarget = target;
                CardTargetId = target;
                CardCropStore.ApplyCropToSliders(target);
            }
            finally
            {
                _syncingCardTarget = false;
            }
        }
        else
        {
            // スライダー変更 → 対象カードへ保存
            CardCropStore.UpsertFromSliders(target);
        }

        VisualTuner.ApplyAll();
    }
}
