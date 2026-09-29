using System.Runtime.CompilerServices;
using Godot;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>希少な心臓の UI（使用可能時の脈打ち・発動ホバー）。</summary>
public static class HeartRelicUi
{
    private const string LocTable = "relics";
    private const string ActivateTitleKey = "HYPNOSISCREATOR-HEART_ACTIVATE_HOVER.title";
    private const string ActivateDescriptionKey = "HYPNOSISCREATOR-HEART_ACTIVATE_HOVER.description";
    private const string LegacyOverlayName = "HcHeartPulseOverlay";

    /// <summary>約 45 BPM。二拍のあと周期の半分以上を静止にする。</summary>
    private const float PeriodSec = 1.3f;
    private const float StrongPeak = 1.12f;
    private const float WeakPeak = 1.07f;
    private const float StrongStart = 0f;
    private const float StrongDuration = 0.22f;
    private const float WeakStart = 0.28f;
    private const float WeakDuration = 0.18f;

    /// <summary>詳細画面の使用可能ラベル用。バー上のアイコン色は変えない。</summary>
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

    /// <summary>戦闘中に今すぐ右クリック発動できるときだけ脈打つ。</summary>
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

        if (Targets.TryGetValue(holder, out var previous))
        {
            ResetScaleIfPulsing(previous);
            Targets.Remove(holder);
        }

        // 心臓以外のレリックは触らない（本家のホバー拡大などを毎フレーム上書きしないため）
        if (model is not EnemyHeartRelic) return;

        FreeLegacyOverlay(icon);
        Targets.Add(holder, new PulseTarget(icon, model, player));
    }

    private static void OnProcessFrame()
    {
        // ハンドラ内では Targets を変更しないので、毎フレームのコピーを作らず直接列挙する。
        foreach (var entry in Targets)
        {
            if (!GodotObject.IsInstanceValid(entry.Key)) continue;
            ApplyPulse(entry.Value);
        }
    }

    private static void ApplyPulse(PulseTarget target)
    {
        var icon = target.Icon;
        if (icon == null || !GodotObject.IsInstanceValid(icon)) return;

        if (target.Model is not EnemyHeartRelic heart)
        {
            ResetScaleIfPulsing(target);
            return;
        }

        var player = target.Player ?? heart.Owner;
        var owned = HeartRelicActivation.ResolveOwnedHeart(heart, player) ?? heart;
        if (!ShouldHighlightForActivation(owned, player))
        {
            ResetScaleIfPulsing(target);
            return;
        }

        target.Pulsing = true;
        icon.PivotOffset = icon.Size * 0.5f;
        var t = Time.GetTicksMsec() / 1000f % PeriodSec;
        var scale = HeartbeatScale(t);
        icon.Scale = new Vector2(scale, scale);
    }

    /// <summary>脈打ちを止めた1回だけ倍率を戻す。常時 Scale を書くと本家のホバー拡大と競合する。</summary>
    private static void ResetScaleIfPulsing(PulseTarget target)
    {
        if (!target.Pulsing) return;
        target.Pulsing = false;
        var icon = target.Icon;
        if (icon != null && GodotObject.IsInstanceValid(icon))
            icon.Scale = Vector2.One;
    }

    /// <summary>強拍 → 弱拍 → 長い休み。サイン波の単調な拡縮は使わない。</summary>
    private static float HeartbeatScale(float tInPeriod)
    {
        var strong = TriangleBump(tInPeriod, StrongStart, StrongDuration, StrongPeak);
        var weak = TriangleBump(tInPeriod, WeakStart, WeakDuration, WeakPeak);
        return Mathf.Max(strong, weak);
    }

    private static float TriangleBump(float t, float start, float duration, float peak)
    {
        if (t < start || t > start + duration) return 1f;
        var u = (t - start) / duration;
        var tri = u < 0.5f ? u * 2f : (1f - u) * 2f;
        return Mathf.Lerp(1f, peak, tri);
    }

    private static void FreeLegacyOverlay(TextureRect icon)
    {
        var leftover = icon.GetNodeOrNull<TextureRect>(LegacyOverlayName);
        leftover?.QueueFree();
    }

    private sealed class PulseTarget(TextureRect icon, RelicModel? model, Player? player)
    {
        public TextureRect Icon { get; } = icon;
        public RelicModel? Model { get; } = model;
        public Player? Player { get; } = player;
        public bool Pulsing { get; set; }
    }
}
