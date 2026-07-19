using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// Spine の代わりに AnimatedSprite2D 連番で待機／被弾／詠唱／攻撃を再生する。
/// ウォーターマーク隠しは Visuals に付いた chroma_key_material 側（全モーション共用）。
/// 全モーションは同一キャンバス（768×1152）前提でスケール差を出さない。
/// </summary>
public static class CombatFrameAnimator
{
    private const string FramesPathHint = "HypnosisCreator/scenes/creature_visuals/combat_frames";
    private const string CombatPathHint = "HypnosisCreator/images/character/combat";
    private const string HookMeta = "hc_frame_anim_hooked";
    private const string AttackLoopMeta = "hc_attack_loop";
    private const string IdleAnim = "idle_loop";
    private const string AttackAnim = "attack";

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
            ["Attack"] = AttackAnim,
            ["attack"] = AttackAnim
        };

    public static void TryPlay(NCreature? creature, string? trigger)
    {
        if (creature == null || string.IsNullOrWhiteSpace(trigger)) return;

        var sprite = FindCombatSprite(creature);
        if (sprite?.SpriteFrames == null) return;
        if (!IsOurCombatSprite(sprite)) return;

        // 多段ヒット中のループ攻撃は、都度リスタートしない
        if (IsAttackLooping(sprite) && IsAttackTrigger(trigger))
            return;

        if (!TryMapAnim(trigger, sprite.SpriteFrames, out var anim))
            return;

        // 単発攻撃トリガーはループOFFで再生（多段は BeginAttackLoop 側）
        if (anim == AttackAnim)
            sprite.SpriteFrames.SetAnimationLoop(AttackAnim, false);

        EnsureFinishedHook(sprite);
        sprite.Play(anim);
    }

    public static void TryIdle(NCreature? creature) => TryPlay(creature, "Idle");

    /// <summary>多段ヒットなど、攻撃中ずっと指パッチンを回す。</summary>
    public static void BeginAttackLoop(Creature? creature) =>
        BeginAttackLoop(creature?.GetCreatureNode());

    public static bool IsAttackLoopActive(Creature? creature) =>
        IsAttackLoopActive(creature?.GetCreatureNode());

    public static bool IsAttackLoopActive(NCreature? creature)
    {
        var sprite = FindOurSprite(creature);
        return sprite != null && IsAttackLooping(sprite);
    }

    public static void BeginAttackLoop(NCreature? creature)
    {
        var sprite = FindOurSprite(creature);
        if (sprite?.SpriteFrames == null) return;
        if (!sprite.SpriteFrames.HasAnimation(AttackAnim)) return;

        EnsureFinishedHook(sprite);
        sprite.SpriteFrames.SetAnimationLoop(AttackAnim, true);
        sprite.SetMeta(AttackLoopMeta, true);
        if (sprite.Animation != AttackAnim || !sprite.IsPlaying())
            sprite.Play(AttackAnim);
    }

    public static void EndAttackLoop(Creature? creature) =>
        EndAttackLoop(creature?.GetCreatureNode());

    public static void EndAttackLoop(NCreature? creature)
    {
        var sprite = FindOurSprite(creature);
        if (sprite?.SpriteFrames == null) return;

        sprite.RemoveMeta(AttackLoopMeta);
        if (sprite.SpriteFrames.HasAnimation(AttackAnim))
            sprite.SpriteFrames.SetAnimationLoop(AttackAnim, false);

        if (sprite.SpriteFrames.HasAnimation(IdleAnim))
            sprite.Play(IdleAnim);
    }

    private static bool IsAttackTrigger(string trigger) =>
        trigger.Equals("Attack", StringComparison.OrdinalIgnoreCase)
        || trigger.Equals("attack", StringComparison.OrdinalIgnoreCase);

    private static bool IsAttackLooping(AnimatedSprite2D sprite) =>
        sprite.HasMeta(AttackLoopMeta) && (bool)sprite.GetMeta(AttackLoopMeta);

    private static AnimatedSprite2D? FindOurSprite(NCreature? creature)
    {
        if (creature == null) return null;
        var sprite = FindCombatSprite(creature);
        if (sprite?.SpriteFrames == null) return null;
        return IsOurCombatSprite(sprite) ? sprite : null;
    }

    private static bool TryMapAnim(string trigger, SpriteFrames frames, out StringName anim)
    {
        if (TriggerToAnim.TryGetValue(trigger, out var mapped) && frames.HasAnimation(mapped))
        {
            anim = mapped;
            return true;
        }

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
        if (IsAttackLooping(sprite)) return;

        var frames = sprite.SpriteFrames;
        if (frames == null) return;

        var current = sprite.Animation;
        if (current.IsEmpty) return;
        if (frames.GetAnimationLoop(current)) return;
        if (!frames.HasAnimation(IdleAnim)) return;

        sprite.Play(IdleAnim);
    }
}
