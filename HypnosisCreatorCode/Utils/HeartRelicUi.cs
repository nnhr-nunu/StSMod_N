using System.Runtime.CompilerServices;
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
    private const string OverlayName = "HcHeartPulseOverlay";
    private const float PulseHz = 1.35f;

    /// <summary>筋力／弱体のキーワードに近い金色。</summary>
    public static readonly Color ActivatableModulate = new(1f, 0.82f, 0.35f);

    private static readonly ConditionalWeakTable<NRelicInventoryHolder, PulseTarget> Targets = new();
    private static SceneTree? _tree;
    private static bool _started;

    public static void Start()
    {
        if (_started) return;
        _tree = Engine.GetMainLoop() as SceneTree;
        if (_tree == null) return;
        _started = true;
        _tree.ProcessFrame += OnProcessFrame;
    }

    /// <summary>右クリック説明ホバー（発動可否とは無関係に常に表示）。</summary>
    public static bool ShouldShowActivationHover(EnemyHeartRelic heart, Player? player) =>
        heart.IsRareHeart;

    /// <summary>戦闘中に今すぐ右クリック発動できるときだけ点滅する。</summary>
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
        Start();
        if (icon == null || !GodotObject.IsInstanceValid(icon)) return;
        if (!GodotObject.IsInstanceValid(holder)) return;

        Targets.Remove(holder);
        Targets.Add(holder, new PulseTarget(icon, model, player));
    }

    private static void OnProcessFrame()
    {
        foreach (var holder in SnapshotHolders())
        {
            if (!GodotObject.IsInstanceValid(holder)) continue;
            if (!Targets.TryGetValue(holder, out var target)) continue;
            ApplyPulse(target);
        }
    }

    private static List<NRelicInventoryHolder> SnapshotHolders()
    {
        var list = new List<NRelicInventoryHolder>();
        foreach (var entry in Targets)
            list.Add(entry.Key);
        return list;
    }

    private static void ApplyPulse(PulseTarget target)
    {
        var icon = target.Icon;
        if (icon == null || !GodotObject.IsInstanceValid(icon)) return;

        var overlay = EnsureOverlay(icon);
        overlay.Texture = icon.Texture;
        overlay.Position = Vector2.Zero;
        overlay.Size = icon.Size;

        if (target.Model is not EnemyHeartRelic heart)
        {
            overlay.Visible = false;
            return;
        }

        var player = target.Player ?? heart.Owner;
        var owned = HeartRelicActivation.ResolveOwnedHeart(heart, player) ?? heart;
        if (!ShouldHighlightForActivation(owned, player))
        {
            overlay.Visible = false;
            return;
        }

        var wave = (Mathf.Sin(Time.GetTicksMsec() / 1000f * Mathf.Tau * PulseHz) + 1f) * 0.5f;
        overlay.Modulate = new Color(1f, 0.78f, 0.18f, Mathf.Lerp(0.2f, 0.92f, wave));
        overlay.Visible = true;
    }

    private static TextureRect EnsureOverlay(TextureRect icon)
    {
        var existing = icon.GetNodeOrNull<TextureRect>(OverlayName);
        if (existing != null) return existing;

        var overlay = new TextureRect
        {
            Name = OverlayName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ExpandMode = icon.ExpandMode,
            StretchMode = icon.StretchMode,
            Texture = icon.Texture
        };
        overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
        icon.AddChild(overlay);
        overlay.Position = Vector2.Zero;
        overlay.Size = icon.Size;
        return overlay;
    }

    private sealed class PulseTarget(TextureRect icon, RelicModel? model, Player? player)
    {
        public TextureRect Icon { get; } = icon;
        public RelicModel? Model { get; } = model;
        public Player? Player { get; } = player;
    }
}
