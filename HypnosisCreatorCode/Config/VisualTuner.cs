using Godot;

namespace HypnosisCreator.HypnosisCreatorCode.Config;

/// <summary>
/// Mod設定の値を、画面上の立ち絵／選択背景／カード絵へ反映する。
/// </summary>
public static class VisualTuner
{
    private const string ChromaMaterialPath = $"{MainFile.ResPath}/shaders/chroma_key_material.tres";
    private const string CropShaderPath = $"{MainFile.ResPath}/shaders/image_crop.gdshader";
    private const string CardPortraitPathHint = "HypnosisCreator/images/card_portraits";
    private const string IdlePathHint = "HypnosisCreator/images/character/combat/idle";
    private const string SelectBgPathHint = "select_bg";
    private const string BaseOffsetsMeta = "hc_base_offsets";

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

    public static void ApplyChroma()
    {
        var key = HypnosisCreatorConfig.GetChromaKeyColor();
        var similarity = (float)HypnosisCreatorConfig.ChromaSimilarity;
        var smoothness = (float)HypnosisCreatorConfig.ChromaSmoothness;
        var spill = (float)HypnosisCreatorConfig.ChromaSpill;

        if (ResourceLoader.Exists(ChromaMaterialPath))
        {
            var shared = ResourceLoader.Load<ShaderMaterial>(ChromaMaterialPath);
            if (shared != null)
                SetChromaParams(shared, key, similarity, smoothness, spill);
        }

        foreach (var sprite in FindNodes<Sprite2D>())
        {
            if (!LooksLikeCombatIdle(sprite)) continue;
            EnsureChromaMaterial(sprite);
            if (sprite.Material is ShaderMaterial mat)
                SetChromaParams(mat, key, similarity, smoothness, spill);
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
            var path = GetTexturePath(item);
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
    }

    private static void RememberAndApplyControlOffsets(Control control, float ox, float oy)
    {
        if (!control.HasMeta(BaseOffsetsMeta))
        {
            control.SetMeta(BaseOffsetsMeta,
                new Vector4(control.OffsetLeft, control.OffsetTop, control.OffsetRight, control.OffsetBottom));
        }

        var baseOff = (Vector4)control.GetMeta(BaseOffsetsMeta);
        control.OffsetLeft = baseOff.X + ox;
        control.OffsetTop = baseOff.Y + oy;
        control.OffsetRight = baseOff.Z + ox;
        control.OffsetBottom = baseOff.W + oy;
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

    private static void SetChromaParams(ShaderMaterial mat, Color key, float similarity, float smoothness, float spill)
    {
        mat.SetShaderParameter("key_color", key);
        mat.SetShaderParameter("similarity", similarity);
        mat.SetShaderParameter("smoothness", smoothness);
        mat.SetShaderParameter("spill", spill);
    }

    private static void SetCropParams(ShaderMaterial mat, Vector2 offset, float zoom)
    {
        mat.SetShaderParameter("offset", offset);
        mat.SetShaderParameter("zoom", zoom);
    }

    private static bool LooksLikeCombatIdle(Sprite2D sprite)
    {
        if (sprite.Name == "Visuals") return true;
        var path = sprite.Texture?.ResourcePath ?? "";
        return path.Contains(IdlePathHint, StringComparison.OrdinalIgnoreCase);
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
}
