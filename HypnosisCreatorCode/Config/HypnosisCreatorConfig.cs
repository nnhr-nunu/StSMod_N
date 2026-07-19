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
    public static double ChromaSimilarity { get; set; } = 0.18;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaSmoothness { get; set; } = 0.06;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaSpill { get; set; } = 0.18;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaKeyR { get; set; } = 0.03;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaKeyG { get; set; } = 0.63;

    [ConfigSlider(0.0, 1.0, 0.01, Format = "{0:0.00}")]
    public static double ChromaKeyB { get; set; } = 0.21;

    // --- キャラ選択 1枚背景（単位: px。Yプラス＝画面下へ） ---

    [ConfigSection("SelectBackground")]
    [ConfigSlider(-400, 400, 1, Format = "{0:0}px")]
    public static double SelectBgOffsetX { get; set; }

    [ConfigSlider(-400, 400, 1, Format = "{0:0}px")]
    public static double SelectBgOffsetY { get; set; }

    [ConfigSlider(0.5, 3.0, 0.01, Format = "{0:0.00}x")]
    public static double SelectBgZoom { get; set; } = 1.0;

    // --- カード絵（個別: CardTargetId で対象を指定） ---

    [ConfigSection("CardArt")]
    [ConfigTextInput(@"[A-Za-z0-9_\-]*")]
    public static string CardTargetId { get; set; } = CardCropStore.DefaultKey;

    [ConfigSlider(-0.5, 0.5, 0.01, Format = "{0:0.00}")]
    public static double CardOffsetX { get; set; }

    [ConfigSlider(-0.5, 0.5, 0.01, Format = "{0:0.00}")]
    public static double CardOffsetY { get; set; }

    [ConfigSlider(0.5, 3.0, 0.01, Format = "{0:0.00}x")]
    public static double CardZoom { get; set; } = 1.0;

    /// <summary>カードごとの切り抜き保存領域（UI非表示）。</summary>
    [ConfigHideInUI]
    public static string CardCropOverridesJson { get; set; } = "{}";

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
