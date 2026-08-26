using Godot;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>希少な心臓の UI（使用可能時の明滅・発動ホバー）。</summary>
public static class HeartRelicUi
{
    private const string LocTable = "relics";
    private const string ActivateTitleKey = "HYPNOSISCREATOR-HEART_ACTIVATE_HOVER.title";
    private const string ActivateDescriptionKey = "HYPNOSISCREATOR-HEART_ACTIVATE_HOVER.description";
    private const string DriverMeta = "hc_heart_pulse_driver";
    private const float PulseHz = 1.35f;

    /// <summary>筋力／弱体のキーワードに近い金色。</summary>
    public static readonly Color ActivatableModulate = new(1f, 0.82f, 0.35f);

    private static readonly Color PulseDim = new(0.55f, 0.28f, 0.04f);
    private static readonly Color PulseBright = new(1.35f, 1.05f, 0.45f);

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

    public static void ApplyHolderVisual(
        NRelicInventoryHolder holder,
        RelicModel? model,
        TextureRect? icon,
        Player? player)
    {
        if (icon == null || !GodotObject.IsInstanceValid(icon)) return;
        if (!GodotObject.IsInstanceValid(holder)) return;

        var driver = EnsureDriver(holder, icon);
        driver.Bind(icon, model, player);
    }

    private static HeartRelicPulseDriver EnsureDriver(NRelicInventoryHolder holder, TextureRect icon)
    {
        if (holder.HasMeta(DriverMeta)
            && holder.GetMeta(DriverMeta).AsGodotObject() is HeartRelicPulseDriver existing
            && GodotObject.IsInstanceValid(existing))
        {
            existing.Icon = icon;
            return existing;
        }

        var driver = new HeartRelicPulseDriver { Name = "HcHeartPulseDriver", Icon = icon };
        holder.AddChild(driver);
        holder.SetMeta(DriverMeta, driver);
        return driver;
    }

    internal static Color PulseColorNow()
    {
        var wave = (Mathf.Sin(Time.GetTicksMsec() / 1000f * Mathf.Tau * PulseHz) + 1f) * 0.5f;
        return PulseDim.Lerp(PulseBright, wave);
    }
}

/// <summary>
/// 本家 <c>RefreshStatus</c> がアイコン色を白／灰に戻すため、Tween ではなく毎フレーム上書きする。
/// </summary>
internal sealed partial class HeartRelicPulseDriver : Node
{
    public TextureRect? Icon;
    private RelicModel? _model;
    private Player? _player;

    public void Bind(TextureRect icon, RelicModel? model, Player? player)
    {
        Icon = icon;
        _model = model;
        _player = player;
    }

    public override void _Process(double delta)
    {
        _ = delta;
        if (Icon == null || !GodotObject.IsInstanceValid(Icon))
        {
            QueueFree();
            return;
        }

        if (_model is not EnemyHeartRelic heart) return;
        var player = _player ?? heart.Owner;
        var owned = HeartRelicActivation.ResolveOwnedHeart(heart, player) ?? heart;
        if (!HeartRelicUi.ShouldHighlightForActivation(owned, player)) return;

        Icon.Modulate = HeartRelicUi.PulseColorNow();
    }
}
