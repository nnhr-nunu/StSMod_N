using Godot;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>希少な心臓の UI（金色ハイライト・使用可能時の明滅・発動ホバー）。</summary>
public static class HeartRelicUi
{
    private const string LocTable = "relics";
    private const string ActivateTitleKey = "HYPNOSISCREATOR-HEART_ACTIVATE_HOVER.title";
    private const string ActivateDescriptionKey = "HYPNOSISCREATOR-HEART_ACTIVATE_HOVER.description";
    private const string PulseTweenMeta = "hc_heart_pulse_tween";
    private const float PulseHalfSeconds = 0.45f;

    /// <summary>筋力／弱体のキーワードに近い金色。</summary>
    public static readonly Color ActivatableModulate = new(1f, 0.82f, 0.35f);

    /// <summary>右クリック説明ホバー（発動可否とは無関係に常に表示）。</summary>
    public static bool ShouldShowActivationHover(EnemyHeartRelic heart, Player? player) =>
        heart.IsRareHeart;

    /// <summary>戦闘中に今すぐ右クリック発動できるときだけ金色にする。</summary>
    public static bool ShouldHighlightForActivation(EnemyHeartRelic heart, Player? player) =>
        HeartRelicActivation.ShouldHighlight(heart, player);

    public static IHoverTip CreateActivationHoverTip() =>
        new HoverTip(
            new LocString(LocTable, ActivateTitleKey),
            new LocString(LocTable, ActivateDescriptionKey),
            icon: null);

    public static void ApplyHolderVisual(RelicModel? model, TextureRect? icon, Player? player)
    {
        if (icon == null || !GodotObject.IsInstanceValid(icon)) return;

        if (model is EnemyHeartRelic heart && ShouldHighlightForActivation(heart, player))
            StartPulse(icon);
        else
            StopPulse(icon);
    }

    /// <summary>
    /// Minty Spire 2 のレリックリマインダーと同様、Tween でアルファ／明度を往復させて「今使える」を示す。
    /// </summary>
    private static void StartPulse(TextureRect icon)
    {
        if (icon.HasMeta(PulseTweenMeta)
            && icon.GetMeta(PulseTweenMeta).AsGodotObject() is Tween running
            && GodotObject.IsInstanceValid(running)
            && running.IsRunning())
        {
            return;
        }

        StopPulse(icon);

        try
        {
            var tween = icon.CreateTween();
            tween.SetLoops();
            tween.SetTrans(Tween.TransitionType.Sine);
            tween.SetEase(Tween.EaseType.InOut);
            tween.TweenMethod(
                Callable.From<float>(t => ApplyPulseFrame(icon, t)),
                0.12f,
                0.88f,
                PulseHalfSeconds);
            tween.TweenMethod(
                Callable.From<float>(t => ApplyPulseFrame(icon, t)),
                0.88f,
                0.12f,
                PulseHalfSeconds);
            icon.SetMeta(PulseTweenMeta, tween);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Heart relic pulse failed: {e.Message}");
        }
    }

    private static void ApplyPulseFrame(TextureRect icon, float t)
    {
        if (!GodotObject.IsInstanceValid(icon)) return;
        icon.Modulate = ActivatableModulate.Lerp(Colors.White, t);
    }

    private static void StopPulse(TextureRect icon)
    {
        if (!icon.HasMeta(PulseTweenMeta)) return;
        if (icon.GetMeta(PulseTweenMeta).AsGodotObject() is Tween tween
            && GodotObject.IsInstanceValid(tween))
        {
            tween.Kill();
        }

        icon.RemoveMeta(PulseTweenMeta);
    }
}
