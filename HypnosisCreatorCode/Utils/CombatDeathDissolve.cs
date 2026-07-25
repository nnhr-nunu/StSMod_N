using Godot;
using HypnosisCreator.HypnosisCreatorCode;
using HypnosisCreator.HypnosisCreatorCode.Config;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 敗北時に立ち絵をノイズ・ディゾルブで消滅させる（die 連番素材なし前提）。
/// chroma_key マテリアルの dissolve_amount を Tween する。
/// </summary>
public static class CombatDeathDissolve
{
    private const string ChromaMaterialPath = $"{MainFile.ResPath}/shaders/chroma_key_material.tres";

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

    /// <summary>戦闘立ち絵ノードからディゾルブを開始（既に開始済みなら false）。</summary>
    public static bool TryBegin(AnimatedSprite2D? sprite)
    {
        if (sprite == null || !GodotObject.IsInstanceValid(sprite)) return false;
        if (IsDissolving(sprite)) return true;
        Begin(sprite);
        return IsDissolving(sprite);
    }

    public static void Begin(AnimatedSprite2D sprite)
    {
        try
        {
            if (!GodotObject.IsInstanceValid(sprite)) return;
            if (IsDissolving(sprite)) return;

            EnsureChromaMaterial(sprite);
            if (sprite.Material is not ShaderMaterial sourceMat)
            {
                MainFile.Logger.Warn("CombatDeathDissolve: ShaderMaterial not found on combat sprite.");
                return;
            }

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
            // shader_parameter の TweenProperty は環境によって効かないため TweenMethod を使う
            tween.TweenMethod(
                Callable.From<float>(v => mat.SetShaderParameter("dissolve_amount", v)),
                0f,
                1f,
                DurationSeconds);
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

    private static void EnsureChromaMaterial(AnimatedSprite2D sprite)
    {
        if (sprite.Material is ShaderMaterial existing &&
            existing.Shader?.ResourcePath?.Contains("chroma_key") == true)
            return;

        if (!ResourceLoader.Exists(ChromaMaterialPath)) return;
        var shared = ResourceLoader.Load<ShaderMaterial>(ChromaMaterialPath);
        if (shared == null) return;
        sprite.Material = (ShaderMaterial)shared.Duplicate();
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
