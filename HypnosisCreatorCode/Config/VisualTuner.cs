using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace HypnosisCreator.HypnosisCreatorCode.Config;

/// <summary>
/// Mod設定の値を、画面上の立ち絵／選択背景／カード絵へ反映する。
/// </summary>
public static class VisualTuner
{
    private const string ChromaMaterialPath = $"{MainFile.ResPath}/shaders/chroma_key_material.tres";
    private const string CropShaderPath = $"{MainFile.ResPath}/shaders/image_crop.gdshader";
    private const string CardPortraitPathHint = "HypnosisCreator/images/card_portraits";
    /// <summary>待機・被弾など戦闘モーション共通（同じ右下WM帯をシェーダーで隠す）。</summary>
    private const string CombatVisualPathHint = "HypnosisCreator/images/character/combat";
    private const string MerchantVisualPathHint = "HypnosisCreator/images/character/merchant";
    private const string SelectBgPathHint = "select_bg";
    private const string BaseOffsetsMeta = "hc_base_offsets";

    /// <summary>立ち絵は Flip H ON 前提（Hailuo 右下ロゴ → UV 左）。</summary>
    private const float WatermarkOnUvLeft = 1.0f;

    private static readonly FieldInfo? NCardPortraitField =
        AccessTools.Field(typeof(NCard), "_portrait");

    private static readonly FieldInfo? NCardAncientPortraitField =
        AccessTools.Field(typeof(NCard), "_ancientPortrait");

    private static Shader? _cropShader;

    public static void ApplyAll()
    {
        try
        {
            ApplyChroma();
            ApplySelectBackground();
            ApplyCardArt();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"VisualTuner.ApplyAll failed: {ex.Message}");
        }
    }

    /// <summary>
    /// カード絵差し替え直後に呼ぶ。ライブラリ一覧では本家表示のままにし、
    /// それ以外ではテクスチャに合わせたクロップを都度当てる（使い回しホルダーのズレ防止）。
    /// </summary>
    public static void ApplyCardPortraitCrop(NCard card)
    {
        try
        {
            if (IsUnderCardLibrary(card))
            {
                ClearPortraitField(card, NCardPortraitField);
                ClearPortraitField(card, NCardAncientPortraitField);
                return;
            }

            var map = CardCropStore.LoadAll();
            ApplyPortraitField(card, NCardPortraitField, map);
            ApplyPortraitField(card, NCardAncientPortraitField, map);
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"VisualTuner.ApplyCardPortraitCrop failed: {ex.Message}");
        }
    }

    private static void ClearPortraitField(NCard card, FieldInfo? field)
    {
        if (field?.GetValue(card) is TextureRect portrait)
            ClearCropMaterial(portrait);
    }

    private static void ApplyPortraitField(NCard card, FieldInfo? field, Dictionary<string, CardCropStore.Crop> map)
    {
        if (field?.GetValue(card) is not TextureRect portrait)
            return;

        ApplyCropToItem(portrait, map);
    }

    public static void ApplyChroma()
    {
        // 旧デフォルト（靴欠けやすい）が設定に残っているときは新デフォルトへ寄せる
        if (IsApprox(HypnosisCreatorConfig.ChromaSimilarity, 0.22)
            && IsApprox(HypnosisCreatorConfig.ChromaSmoothness, 0.08)
            && IsApprox(HypnosisCreatorConfig.ChromaSpill, 0.25))
        {
            HypnosisCreatorConfig.ChromaSimilarity = 0.18;
            HypnosisCreatorConfig.ChromaSmoothness = 0.06;
            HypnosisCreatorConfig.ChromaSpill = 0.18;
        }

        var key = HypnosisCreatorConfig.GetChromaKeyColor();
        var similarity = (float)HypnosisCreatorConfig.ChromaSimilarity;
        var smoothness = (float)HypnosisCreatorConfig.ChromaSmoothness;
        var spill = (float)HypnosisCreatorConfig.ChromaSpill;
        var wmBottom = (float)HypnosisCreatorConfig.WatermarkCropBottom;
        var wmSide = (float)HypnosisCreatorConfig.WatermarkCropSide;
        var wmLogo = (float)HypnosisCreatorConfig.WatermarkLogoStrength;

        if (ResourceLoader.Exists(ChromaMaterialPath))
        {
            var shared = ResourceLoader.Load<ShaderMaterial>(ChromaMaterialPath);
            if (shared != null)
                SetChromaParams(shared, key, similarity, smoothness, spill, wmBottom, wmSide, wmLogo);
        }

        foreach (var item in FindChromaVisuals())
        {
            EnsureChromaMaterial(item);
            if (item.Material is ShaderMaterial mat)
                SetChromaParams(mat, key, similarity, smoothness, spill, wmBottom, wmSide, wmLogo);
        }
    }

    public static void ApplySelectBackground()
    {
        // 単位 px。Yプラス＝画面下方向へずらす（顔が見切れるとき正の値）
        var ox = (float)HypnosisCreatorConfig.SelectBgOffsetX;
        var oy = (float)HypnosisCreatorConfig.SelectBgOffsetY;
        var zoom = (float)HypnosisCreatorConfig.SelectBgZoom;

        var found = 0;
        foreach (var rect in FindSelectBackgrounds())
        {
            found++;
            RememberAndApplyControlOffsets(rect, ox, oy);

            EnsureCropMaterial(rect);
            if (rect.Material is ShaderMaterial mat)
            {
                // 位置は Control オフセットで動かす。シェーダーはズーム専用。
                SetCropParams(mat, Vector2.Zero, zoom);
            }
        }

        if (found == 0)
            MainFile.Logger.Info("VisualTuner: select background TextureRect not found (open character select to adjust).");
    }

    public static void ApplyCardArt()
    {
        var map = CardCropStore.LoadAll();

        foreach (var item in FindCanvasItemsWithCardPortrait())
        {
            if (IsUnderCardLibrary(item))
            {
                ClearCropMaterial(item);
                continue;
            }

            ApplyCropToItem(item, map);
        }
    }

    /// <summary>カードライブラリ内のノード追加では全再適用を起こさない（一覧のちらつき／負荷防止）。</summary>
    public static bool IsCardPortraitNode(Node node)
    {
        var path = node switch
        {
            TextureRect r => r.Texture?.ResourcePath ?? "",
            Sprite2D s => s.Texture?.ResourcePath ?? "",
            _ => ""
        };
        return path.Contains(CardPortraitPathHint, StringComparison.OrdinalIgnoreCase);
    }

    private static void ApplyCropToItem(CanvasItem item, Dictionary<string, CardCropStore.Crop> map)
    {
        var path = GetTexturePath(item);
        if (!path.Contains(CardPortraitPathHint, StringComparison.OrdinalIgnoreCase))
        {
            ClearCropMaterial(item);
            return;
        }

        var key = CardCropStore.KeyFromTexturePath(path);
        var crop = CardCropStore.Get(key, map);

        EnsureCropMaterial(item);
        if (item.Material is ShaderMaterial mat)
        {
            // UI は「Yプラス＝画面下へ」。UV オフセットは符号が逆なので反転する。
            SetCropParams(mat,
                new Vector2((float)crop.OffsetX, -(float)crop.OffsetY),
                (float)crop.Zoom);
        }
    }

    private static void ClearCropMaterial(CanvasItem item)
    {
        if (item.Material is ShaderMaterial mat &&
            mat.Shader?.ResourcePath?.Contains("image_crop", StringComparison.OrdinalIgnoreCase) == true)
        {
            item.Material = null;
        }
    }

    private static bool IsUnderCardLibrary(Node node)
    {
        for (var p = node; p != null; p = p.GetParent())
        {
            if (p is NCardLibrary or NCardLibraryGrid)
                return true;
        }

        return false;
    }

    private static void RememberAndApplyControlOffsets(Control control, float ox, float oy)
    {
        if (!control.HasMeta(BaseOffsetsMeta))
        {
            control.SetMeta(BaseOffsetsMeta,
                new Vector4(control.OffsetLeft, control.OffsetTop, control.OffsetRight, control.OffsetBottom));
        }

        var baseOff = (Vector4)control.GetMeta(BaseOffsetsMeta);
        // 四辺を逆方向にずらしてサイズを保ったまま平行移動する（左右同加算だと縮小だけになる）
        control.OffsetLeft = baseOff.X + ox;
        control.OffsetTop = baseOff.Y + oy;
        control.OffsetRight = baseOff.Z - ox;
        control.OffsetBottom = baseOff.W - oy;
    }

    private static void EnsureChromaMaterial(CanvasItem item)
    {
        if (item.Material is ShaderMaterial existing &&
            existing.Shader?.ResourcePath?.Contains("chroma_key") == true)
            return;

        if (!ResourceLoader.Exists(ChromaMaterialPath)) return;
        var shared = ResourceLoader.Load<ShaderMaterial>(ChromaMaterialPath);
        if (shared == null) return;
        item.Material = (ShaderMaterial)shared.Duplicate();
    }

    private static void EnsureCropMaterial(CanvasItem item)
    {
        if (item.Material is ShaderMaterial existing &&
            existing.Shader?.ResourcePath?.Contains("image_crop") == true)
            return;

        _cropShader ??= ResourceLoader.Exists(CropShaderPath)
            ? ResourceLoader.Load<Shader>(CropShaderPath)
            : null;
        if (_cropShader == null) return;

        item.Material = new ShaderMaterial { Shader = _cropShader };
    }

    private static void SetChromaParams(
        ShaderMaterial mat,
        Color key,
        float similarity,
        float smoothness,
        float spill,
        float wmBottom,
        float wmSide,
        float wmLogo)
    {
        mat.SetShaderParameter("key_color", key);
        mat.SetShaderParameter("similarity", similarity);
        mat.SetShaderParameter("smoothness", smoothness);
        mat.SetShaderParameter("spill", spill);
        // パワー詠唱の粒子が青っぽく残るのを抑える（シェーダー既定と同じ）
        mat.SetShaderParameter("sparkle_whiten", 0.9f);
        // 画像は改変せず、全戦闘モーションで同じ帯を透明化（Mod設定スライダー）
        mat.SetShaderParameter("wm_crop_bottom", wmBottom);
        mat.SetShaderParameter("wm_crop_side", wmSide);
        mat.SetShaderParameter("wm_on_uv_left", WatermarkOnUvLeft);
        mat.SetShaderParameter("wm_logo_strength", wmLogo);
        // dissolve_amount は敗北演出中に触らない（VisualTuner 再適用で途中リセットしない）
    }

    private static void SetCropParams(ShaderMaterial mat, Vector2 offset, float zoom)
    {
        mat.SetShaderParameter("offset", offset);
        mat.SetShaderParameter("zoom", zoom);
    }

    private static IEnumerable<CanvasItem> FindChromaVisuals()
    {
        foreach (var sprite in FindNodes<Sprite2D>())
        {
            if (LooksLikeChromaVisual(sprite.Name, sprite.Texture?.ResourcePath))
                yield return sprite;
        }

        foreach (var sprite in FindNodes<AnimatedSprite2D>())
        {
            if (LooksLikeChromaVisual(sprite.Name, GetAnimatedSpritePathHint(sprite)))
                yield return sprite;
        }
    }

    private static bool LooksLikeChromaVisual(StringName name, string? pathHint)
    {
        if (name == "Visuals") return true;
        if (pathHint == null) return false;
        return pathHint.Contains(CombatVisualPathHint, StringComparison.OrdinalIgnoreCase)
               || pathHint.Contains(MerchantVisualPathHint, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetAnimatedSpritePathHint(AnimatedSprite2D sprite)
    {
        var frames = sprite.SpriteFrames;
        if (frames == null) return "";

        var anim = sprite.Animation;
        if (anim.IsEmpty || !frames.HasAnimation(anim))
            return frames.ResourcePath ?? "";

        if (frames.GetFrameCount(anim) <= 0)
            return frames.ResourcePath ?? "";

        return frames.GetFrameTexture(anim, 0)?.ResourcePath
               ?? frames.ResourcePath
               ?? "";
    }

    private static IEnumerable<TextureRect> FindSelectBackgrounds()
    {
        foreach (var rect in FindNodes<TextureRect>())
        {
            var path = rect.Texture?.ResourcePath ?? "";
            if (path.Contains(SelectBgPathHint, StringComparison.OrdinalIgnoreCase))
            {
                yield return rect;
                continue;
            }

            if (rect.Name == "Background" && ParentNameContains(rect, "HypnosisCreator"))
                yield return rect;
        }
    }

    private static bool ParentNameContains(Node node, string needle)
    {
        for (var p = node.GetParent(); p != null; p = p.GetParent())
        {
            if (p.Name.ToString().Contains(needle, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string GetTexturePath(CanvasItem item) => item switch
    {
        TextureRect r => r.Texture?.ResourcePath ?? "",
        Sprite2D s => s.Texture?.ResourcePath ?? "",
        _ => ""
    };

    private static IEnumerable<CanvasItem> FindCanvasItemsWithCardPortrait()
    {
        foreach (var rect in FindNodes<TextureRect>())
        {
            var path = rect.Texture?.ResourcePath ?? "";
            if (path.Contains(CardPortraitPathHint, StringComparison.OrdinalIgnoreCase))
                yield return rect;
        }

        foreach (var sprite in FindNodes<Sprite2D>())
        {
            var path = sprite.Texture?.ResourcePath ?? "";
            if (path.Contains(CardPortraitPathHint, StringComparison.OrdinalIgnoreCase))
                yield return sprite;
        }
    }

    private static IEnumerable<T> FindNodes<T>() where T : Node
    {
        var root = (Engine.GetMainLoop() as SceneTree)?.Root;
        if (root == null) yield break;

        var stack = new Stack<Node>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node is T match)
                yield return match;

            foreach (var child in node.GetChildren())
                stack.Push(child);
        }
    }

    private static bool IsApprox(double a, double b) => Math.Abs(a - b) < 0.005;
}
