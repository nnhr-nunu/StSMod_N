using BaseLib.Config;
using BaseLib.Config.UI;
using Godot;

using HypnosisCreator.HypnosisCreatorCode.Utils;

namespace HypnosisCreator.HypnosisCreatorCode.Config;

/// <summary>
/// Mod設定（Settings → Mod Settings → Hypno Creator）。
/// スライダーを動かすと VisualTuner 経由で即反映され、閉じると自動保存される。
/// </summary>
public sealed class HypnosisCreatorConfig : SimpleModConfig
{
    private static bool _syncingCardTarget;
    private static string _lastCardTarget = CardCropStore.DefaultKey;
    private static bool _syncingRestSiteSeat;
    private static int _lastRestSiteSeat = -1;

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

    /// <summary>マルチロビー専用の Y 補正（ソロの SelectBgOffsetY に加算）。</summary>
    [ConfigSlider(-200, 200, 1, Format = "{0:0}px")]
    public static double SelectBgMultiplayerOffsetY { get; set; } =
        HypnosisCreatorConfigDefaults.SelectBgMultiplayerOffsetY;

    [ConfigButton("ResetSelectBackgroundDefaults")]
    public static void OnResetSelectBackgroundDefaults(ModConfig cfg, NConfigOptionRow row)
    {
        _ = row;
        SelectBgOffsetX = HypnosisCreatorConfigDefaults.SelectBgOffsetX;
        SelectBgOffsetY = HypnosisCreatorConfigDefaults.SelectBgOffsetY;
        SelectBgZoom = HypnosisCreatorConfigDefaults.SelectBgZoom;
        SelectBgMultiplayerOffsetY = HypnosisCreatorConfigDefaults.SelectBgMultiplayerOffsetY;
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

    // --- 篝火：席ごとの位置（マルチ実戦の焚き火でも即反映） ---

    [ConfigSection("RestSitePosition")]
    [ConfigSlider(0, 3, 1, Format = "Seat {0:0}")]
    public static double RestSitePreviewSeat { get; set; } = HypnosisCreatorConfigDefaults.RestSitePreviewSeat;

    [ConfigSlider(-1200, 1200, 1, Format = "{0:0}px")]
    public static double RestSiteOffsetX { get; set; } = HypnosisCreatorConfigDefaults.RestSiteOffsetX;

    [ConfigSlider(-500, 500, 1, Format = "{0:0}px")]
    public static double RestSiteOffsetY { get; set; } = HypnosisCreatorConfigDefaults.RestSiteOffsetY;

    [ConfigHideInUI]
    public static string RestSiteSeatOffsetsJson { get; set; } = "{}";

    [ConfigButton("ResetRestSiteSeatDefault")]
    public static void OnResetRestSiteSeatDefault(ModConfig cfg, NConfigOptionRow row)
    {
        _ = row;
        var seat = RestSiteSeatStore.ClampSeat((int)RestSitePreviewSeat);
        RestSiteSeatStore.RemoveSeat(seat);
        _lastRestSiteSeat = seat;
        RestSiteSeatStore.ApplyToSliders(seat);
        cfg.Changed();
    }

    [ConfigButton("ResetRestSiteDefaults")]
    public static void OnResetRestSiteDefaults(ModConfig cfg, NConfigOptionRow row)
    {
        _ = row;
        RestSiteSeatStore.ResetAllDefaults();
        _lastRestSiteSeat = 0;
        cfg.Changed();
    }

    // --- 篝火：ソロでのマルチ配置プレビュー（実マルチの位置は上の「席ごとの位置」で調整） ---

    [ConfigSection("RestSiteSoloPreview")]
    /// <summary>1＝席0〜3にキャラを並べてマルチ配置をシミュレート。</summary>
    [ConfigSlider(0, 1, 1, Format = "{0:0}")]
    public static double RestSiteSimulateMultiLayout { get; set; } = HypnosisCreatorConfigDefaults.RestSiteSimulateMultiLayout;

    /// <summary>0=空 / 1=Ironclad / 2=Silent / 3=Defect / 4=Regent / 5=Necrobinder / 6=Hypnosis Creator</summary>
    [ConfigSlider(0, 6, 1, Format = "Seat0 {0:0}")]
    public static double RestSiteSimSeat0 { get; set; } = HypnosisCreatorConfigDefaults.RestSiteSimSeat0;

    [ConfigSlider(0, 6, 1, Format = "Seat1 {0:0}")]
    public static double RestSiteSimSeat1 { get; set; } = HypnosisCreatorConfigDefaults.RestSiteSimSeat1;

    [ConfigSlider(0, 6, 1, Format = "Seat2 {0:0}")]
    public static double RestSiteSimSeat2 { get; set; } = HypnosisCreatorConfigDefaults.RestSiteSimSeat2;

    [ConfigSlider(0, 6, 1, Format = "Seat3 {0:0}")]
    public static double RestSiteSimSeat3 { get; set; } = HypnosisCreatorConfigDefaults.RestSiteSimSeat3;

    /// <summary>1＝休憩所でプレビュー席の配置を表示（ソロでも席1〜3を確認）。</summary>
    [ConfigSlider(0, 1, 1, Format = "{0:0}")]
    public static double RestSiteUsePreviewLayout { get; set; } = HypnosisCreatorConfigDefaults.RestSiteUsePreviewLayout;

    public static Color GetChromaKeyColor() =>
        new((float)ChromaKeyR, (float)ChromaKeyG, (float)ChromaKeyB, 1f);

    private static void OnAnyConfigChanged()
    {
        if (_syncingCardTarget || _syncingRestSiteSeat)
        {
            VisualTuner.ApplyAll();
            return;
        }

        SyncRestSiteSeatSliders();
        SyncCardTargetSliders();
        VisualTuner.ApplyAll();
    }

    private static void SyncCardTargetSliders()
    {
        var target = CardCropStore.NormalizeKey(CardTargetId);
        if (target != _lastCardTarget)
        {
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
            CardCropStore.UpsertFromSliders(target);
        }
    }

    private static void SyncRestSiteSeatSliders()
    {
        var seat = RestSiteSeatStore.ClampSeat((int)RestSitePreviewSeat);
        if (seat != _lastRestSiteSeat)
        {
            _syncingRestSiteSeat = true;
            try
            {
                _lastRestSiteSeat = seat;
                RestSitePreviewSeat = seat;
                RestSiteSeatStore.ApplyToSliders(seat);
            }
            finally
            {
                _syncingRestSiteSeat = false;
            }
        }
        else
        {
            RestSiteSeatStore.UpsertFromSliders(seat);
        }
    }
}
