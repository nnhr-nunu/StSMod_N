using Godot;
using HypnosisCreator.HypnosisCreatorCode;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 敗北時に立ち絵をノイズ・ディゾルブで消滅させる（die 連番素材なし前提）。
/// chroma_key マテリアルの dissolve_amount を Tween する。
/// </summary>
public static class CombatDeathDissolve
{
    /// <summary>本家 AnimDie が待つ尺と揃える目安。</summary>
    public const float DurationSeconds = 1.25f;

    private const string DissolvingMeta = "hc_death_dissolve";
    private const string OwnedMatMeta = "hc_dissolve_mat_owned";
    private const string TweenMeta = "hc_death_dissolve_tween";

    public static bool IsDissolving(AnimatedSprite2D sprite) =>
        GodotObject.IsInstanceValid(sprite)
        && sprite.HasMeta(DissolvingMeta)
        && (bool)sprite.GetMeta(DissolvingMeta);

    public static float GetRemainingSeconds(AnimatedSprite2D sprite)
    {
        if (!IsDissolving(sprite)) return 0f;
        if (sprite.Material is not ShaderMaterial mat) return DurationSeconds;

        var amount = mat.GetShaderParameter("dissolve_amount").AsSingle();
        return Math.Clamp((1f - amount) * DurationSeconds, 0f, DurationSeconds);
    }

    public static void Begin(AnimatedSprite2D sprite)
    {
        try
        {
            if (!GodotObject.IsInstanceValid(sprite)) return;
            if (IsDissolving(sprite)) return;
            if (sprite.Material is not ShaderMaterial sourceMat) return;

            // 共有 chroma_key_material を溶かさないようインスタンス専用にする
            var mat = sourceMat;
            if (!sprite.HasMeta(OwnedMatMeta))
            {
                mat = (ShaderMaterial)sourceMat.Duplicate();
                sprite.Material = mat;
                sprite.SetMeta(OwnedMatMeta, true);
            }
            else if (sprite.Material is ShaderMaterial owned)
            {
                mat = owned;
            }

            KillTween(sprite);
            sprite.Pause();
            sprite.SetMeta(DissolvingMeta, true);
            mat.SetShaderParameter("dissolve_amount", 0f);

            var tween = sprite.CreateTween();
            tween.SetEase(Tween.EaseType.In);
            tween.SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(mat, "shader_parameter/dissolve_amount", 1.0, DurationSeconds);
            sprite.SetMeta(TweenMeta, tween);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"CombatDeathDissolve.Begin failed: {e.Message}");
        }
    }

    public static void Reset(AnimatedSprite2D sprite)
    {
        try
        {
            if (!GodotObject.IsInstanceValid(sprite)) return;

            KillTween(sprite);

            if (sprite.HasMeta(DissolvingMeta))
                sprite.RemoveMeta(DissolvingMeta);

            if (sprite.Material is ShaderMaterial mat)
                mat.SetShaderParameter("dissolve_amount", 0f);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"CombatDeathDissolve.Reset failed: {e.Message}");
        }
    }

    private static void KillTween(AnimatedSprite2D sprite)
    {
        if (!sprite.HasMeta(TweenMeta)) return;
        if (sprite.GetMeta(TweenMeta).AsGodotObject() is Tween tw
            && GodotObject.IsInstanceValid(tw))
        {
            tw.Kill();
        }

        sprite.RemoveMeta(TweenMeta);
    }
}
