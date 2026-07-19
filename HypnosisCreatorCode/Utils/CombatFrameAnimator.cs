using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// Spine の代わりに AnimatedSprite2D 連番で待機／被弾／詠唱を再生する。
/// ウォーターマーク隠しは Visuals に付いた chroma_key_material 側（全モーション共用）。
/// </summary>
public static class CombatFrameAnimator
{
    private const string FramesPathHint = "HypnosisCreator/scenes/creature_visuals/combat_frames";
    private const string CombatPathHint = "HypnosisCreator/images/character/combat";
    private const string HookMeta = "hc_frame_anim_hooked";
    private const string IdleAnim = "idle_loop";

    private static readonly Dictionary<string, string> TriggerToAnim =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Idle"] = IdleAnim,
            ["idle"] = IdleAnim,
            ["idle_loop"] = IdleAnim,
            ["Hit"] = "hurt",
            ["Hurt"] = "hurt",
            ["hurt"] = "hurt",
            ["Cast"] = "cast",
            ["cast"] = "cast",
            ["start_cast"] = "cast",
            ["Power"] = "cast",
            ["power"] = "cast",
            ["Attack"] = "attack",
            ["attack"] = "attack"
        };

    public static void TryPlay(NCreature? creature, string? trigger)
    {
        if (creature == null || string.IsNullOrWhiteSpace(trigger)) return;

        var sprite = FindCombatSprite(creature);
        if (sprite?.SpriteFrames == null) return;
        if (!IsOurCombatSprite(sprite)) return;

        if (!TryMapAnim(trigger, sprite.SpriteFrames, out var anim))
            return;

        EnsureFinishedHook(sprite);
        sprite.Play(anim);
    }

    public static void TryIdle(NCreature? creature) => TryPlay(creature, "Idle");

    private static bool TryMapAnim(string trigger, SpriteFrames frames, out StringName anim)
    {
        if (TriggerToAnim.TryGetValue(trigger, out var mapped) && frames.HasAnimation(mapped))
        {
            anim = mapped;
            return true;
        }

        // そのままアニメ名として存在するケース（idle_loop 等）
        if (frames.HasAnimation(trigger))
        {
            anim = trigger;
            return true;
        }

        anim = IdleAnim;
        return false;
    }

    private static AnimatedSprite2D? FindCombatSprite(NCreature creature)
    {
        var visuals = creature.Visuals;
        if (visuals == null) return null;

        var byUnique = visuals.GetNodeOrNull<AnimatedSprite2D>("%Visuals");
        if (byUnique != null) return byUnique;

        var byName = visuals.GetNodeOrNull<AnimatedSprite2D>("Visuals");
        if (byName != null) return byName;

        return visuals.FindChild("Visuals", recursive: true, owned: false) as AnimatedSprite2D;
    }

    private static bool IsOurCombatSprite(AnimatedSprite2D sprite)
    {
        var framesPath = sprite.SpriteFrames?.ResourcePath ?? "";
        if (framesPath.Contains(FramesPathHint, StringComparison.OrdinalIgnoreCase))
            return true;

        // 旧 idle_frames.tres や単体テクスチャでも combat 配下なら対象
        if (framesPath.Contains(CombatPathHint, StringComparison.OrdinalIgnoreCase))
            return true;

        if (sprite.SpriteFrames is { } frames && frames.HasAnimation(IdleAnim))
        {
            var tex = frames.GetFrameCount(IdleAnim) > 0
                ? frames.GetFrameTexture(IdleAnim, 0)?.ResourcePath ?? ""
                : "";
            return tex.Contains(CombatPathHint, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static void EnsureFinishedHook(AnimatedSprite2D sprite)
    {
        if (sprite.HasMeta(HookMeta)) return;
        sprite.AnimationFinished += () => OnAnimationFinished(sprite);
        sprite.SetMeta(HookMeta, true);
    }

    private static void OnAnimationFinished(AnimatedSprite2D sprite)
    {
        if (!GodotObject.IsInstanceValid(sprite)) return;
        var frames = sprite.SpriteFrames;
        if (frames == null) return;

        var current = sprite.Animation;
        if (current.IsEmpty) return;
        if (frames.GetAnimationLoop(current)) return;
        if (!frames.HasAnimation(IdleAnim)) return;

        sprite.Play(IdleAnim);
    }
}
