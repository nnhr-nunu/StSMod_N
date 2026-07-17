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
    private const string SelectBgPathHint = "HypnosisCreator/images/char_select/select_bg";

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

        // 共有マテリアル（シーンに載っているもの）を更新
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
        var offset = new Vector2(
            (float)HypnosisCreatorConfig.SelectBgOffsetX,
            (float)HypnosisCreatorConfig.SelectBgOffsetY);
        var zoom = (float)HypnosisCreatorConfig.SelectBgZoom;

        foreach (var rect in FindNodes<TextureRect>())
        {
            if (!LooksLikeSelectBackground(rect)) continue;
            EnsureCropMaterial(rect);
            if (rect.Material is ShaderMaterial mat)
                SetCropParams(mat, offset, zoom);
        }
    }

    public static void ApplyCardArt()
    {
        var offset = new Vector2(
            (float)HypnosisCreatorConfig.CardOffsetX,
            (float)HypnosisCreatorConfig.CardOffsetY);
        var zoom = (float)HypnosisCreatorConfig.CardZoom;

        foreach (var item in FindCanvasItemsWithCardPortrait())
        {
            EnsureCropMaterial(item);
            if (item.Material is ShaderMaterial mat)
                SetCropParams(mat, offset, zoom);
        }
    }

    private static void EnsureChromaMaterial(CanvasItem item)
    {
        if (item.Material is ShaderMaterial existing &&
            existing.Shader?.ResourcePath?.Contains("chroma_key") == true)
            return;

        if (!ResourceLoader.Exists(ChromaMaterialPath)) return;
        var shared = ResourceLoader.Load<ShaderMaterial>(ChromaMaterialPath);
        if (shared == null) return;
        // 個体ごとに複製して、他キャラへ影響しないようにする
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

    private static bool LooksLikeSelectBackground(TextureRect rect)
    {
        if (rect.Name == "Background" && rect.GetParent()?.Name == "HypnosisCreatorBg")
            return true;
        var path = rect.Texture?.ResourcePath ?? "";
        return path.Contains(SelectBgPathHint, StringComparison.OrdinalIgnoreCase);
    }

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
