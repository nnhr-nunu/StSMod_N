namespace HypnosisCreator.HypnosisCreatorCode.Config;

/// <summary>Mod設定の配布時デフォルト（HypnosisCreatorConfig の初期値とリセット先）。</summary>
internal static class HypnosisCreatorConfigDefaults
{
    public const double ChromaSimilarity = 0.21;
    public const double ChromaSmoothness = 0.13;
    public const double ChromaSpill = 0.18;
    public const double ChromaKeyR = 0.03;
    public const double ChromaKeyG = 0.63;
    public const double ChromaKeyB = 0.21;

    public const double WatermarkCropBottom = 0.055;
    public const double WatermarkCropSide = 0.43;
    public const double WatermarkLogoStrength = 1.0;
    /// <summary>0＝テクスチャ右下（flip_h 立ち絵のロゴ位置）。1＝テクスチャ左下。</summary>
    public const double WatermarkMaskOnUvLeft = 0.0;

    public const double SelectBgOffsetX = 156;
    public const double SelectBgOffsetY = 197;
    public const double SelectBgZoom = 1.13;

    public const double CardOffsetX = 0;
    public const double CardOffsetY = 0;
    public const double CardZoom = 1.0;

    public const double RestSitePreviewSeat = 0;
    public const double RestSiteUsePreviewLayout = 0;
    public const double RestSiteOffsetX = 0;
    /// <summary>席0（ソロ）の初期値。Yプラス＝下へ。</summary>
    public const double RestSiteSeat0OffsetY = 24;
    public const double RestSiteOffsetY = RestSiteSeat0OffsetY;
}
