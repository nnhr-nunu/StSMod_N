using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 飛翔（本家 <c>SoarPower</c> / 本mod <see cref="Powers.ThisTurnSoarPower"/>）中の見た目演出。
/// 立ち絵を少し浮かせて上下させ、足元に黒い半透明の影を出す（浮くほど影は小さく薄くなる）。
/// どちらの飛翔でも同じ演出にするため、パワー種別に依存しないユーティリティとして独立させる。
/// </summary>
public static class SoarFloatVisual
{
    private const string ActiveMeta = "hc_soar_float_active";
    private const string TweenMeta = "hc_soar_float_tween";
    private const string BaseYMeta = "hc_soar_float_base_y";
    private const string ShadowMeta = "hc_soar_float_shadow";

    /// <summary>浮遊の高さ（ローカル px）。「少し」浮く程度に抑える。</summary>
    private const float BobAmplitude = 10f;

    /// <summary>上昇→下降1往復の秒数。</summary>
    private const float BobPeriodSeconds = 1.6f;

    private const float ShadowRadiusX = 34f;
    private const float ShadowRadiusY = 12f;
    private const float ShadowBaseAlpha = 0.35f;
    private const float ShadowShrinkAtPeak = 0.3f;
    private const float ShadowFadeAtPeak = 0.35f;

    public static void Begin(Creature? creature)
    {
        if (creature is not { IsAlive: true }) return;

        try
        {
            var node = creature.GetCreatureNode();
            var visuals = node?.Visuals;
            if (node == null || visuals == null || !GodotObject.IsInstanceValid(visuals)) return;
            if (IsActive(visuals)) return;

            var shadow = CreateShadow(node);
            if (shadow == null) return;

            visuals.SetMeta(ActiveMeta, true);
            visuals.SetMeta(BaseYMeta, (double)visuals.Position.Y);
            visuals.SetMeta(ShadowMeta, shadow);

            var tween = visuals.CreateTween();
            tween.SetLoops();
            tween.SetTrans(Tween.TransitionType.Linear);
            tween.TweenMethod(
                Callable.From<float>(t => ApplyFrame(visuals, shadow, t)),
                0f, Mathf.Tau, BobPeriodSeconds);
            visuals.SetMeta(TweenMeta, tween);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"SoarFloatVisual.Begin failed: {e.Message}");
        }
    }

    public static void End(Creature? creature)
    {
        if (creature == null) return;

        try
        {
            var visuals = creature.GetCreatureNode()?.Visuals;
            if (visuals == null || !GodotObject.IsInstanceValid(visuals)) return;
            if (!IsActive(visuals)) return;

            if (visuals.HasMeta(TweenMeta) &&
                visuals.GetMeta(TweenMeta).AsGodotObject() is Tween tween &&
                GodotObject.IsInstanceValid(tween))
            {
                tween.Kill();
            }

            var baseY = visuals.HasMeta(BaseYMeta) ? (float)visuals.GetMeta(BaseYMeta).AsDouble() : visuals.Position.Y;
            visuals.Position = new Vector2(visuals.Position.X, baseY);

            if (visuals.HasMeta(ShadowMeta) &&
                visuals.GetMeta(ShadowMeta).AsGodotObject() is Node2D shadow &&
                GodotObject.IsInstanceValid(shadow))
            {
                shadow.QueueFree();
            }

            visuals.RemoveMeta(ActiveMeta);
            visuals.RemoveMeta(TweenMeta);
            visuals.RemoveMeta(BaseYMeta);
            visuals.RemoveMeta(ShadowMeta);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"SoarFloatVisual.End failed: {e.Message}");
        }
    }

    private static bool IsActive(Node2D visuals) =>
        visuals.HasMeta(ActiveMeta) && (bool)visuals.GetMeta(ActiveMeta);

    /// <summary>0→2πのtから0..1..0の浮遊量を作り、立ち絵位置と影の縮小・フェードへ反映する。</summary>
    private static void ApplyFrame(Node2D visuals, Polygon2D shadow, float t)
    {
        if (!GodotObject.IsInstanceValid(visuals) || !GodotObject.IsInstanceValid(shadow)) return;

        var lift = 0.5f + 0.5f * Mathf.Sin(t);
        var baseY = visuals.HasMeta(BaseYMeta) ? (float)visuals.GetMeta(BaseYMeta).AsDouble() : visuals.Position.Y;

        visuals.Position = new Vector2(visuals.Position.X, baseY - BobAmplitude * lift);
        shadow.Scale = Vector2.One * (1f - ShadowShrinkAtPeak * lift);
        shadow.Modulate = new Color(0f, 0f, 0f, ShadowBaseAlpha * (1f - ShadowFadeAtPeak * lift));
    }

    /// <summary>足元（ヒットボックス下端）に黒い半透明の楕円を1枚だけ生成する。</summary>
    private static Polygon2D? CreateShadow(NCreature node)
    {
        var hit = node.Hitbox;
        if (hit == null || !GodotObject.IsInstanceValid(hit)) return null;

        var shadow = new Polygon2D
        {
            Polygon = BuildEllipsePoints(ShadowRadiusX, ShadowRadiusY),
            Color = new Color(0f, 0f, 0f, ShadowBaseAlpha),
            ZIndex = -10,
            ZAsRelative = false,
            Position = hit.Position + new Vector2(0f, hit.Size.Y * 0.5f)
        };

        GodotTreeExtensions.AddChildSafely(node, shadow);
        return shadow;
    }

    private static Vector2[] BuildEllipsePoints(float radiusX, float radiusY, int segments = 24)
    {
        var points = new Vector2[segments];
        for (var i = 0; i < segments; i++)
        {
            var angle = Mathf.Tau * i / segments;
            points[i] = new Vector2(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusY);
        }

        return points;
    }
}
