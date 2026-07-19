using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using HypnosisCreator.HypnosisCreatorCode;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// Spine の代わりに AnimatedSprite2D 連番で待機／被弾／詠唱／攻撃を再生する。
/// 敗北（Dead）は die 連番ではなく CombatDeathDissolve（シェーダー消滅）。
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
    private const string CastAnim = "cast";
    private const string HurtAnim = "hurt";

    /// <summary>combat_frames.tres の frame数 / speed と揃える（Character の AnimDelay 用）。</summary>
    public const float AttackAnimSeconds = 6f / 14f;
    public const float CastAnimSeconds = 24f / 14f;
    public const float HurtAnimSeconds = 17f / 28f;

    private static readonly Dictionary<string, string> TriggerToAnim =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Idle"] = IdleAnim,
            ["idle"] = IdleAnim,
            ["idle_loop"] = IdleAnim,
            ["Hit"] = HurtAnim,
            ["Hurt"] = HurtAnim,
            ["hurt"] = HurtAnim,
            ["Cast"] = CastAnim,
            ["cast"] = CastAnim,
            ["start_cast"] = CastAnim,
            ["Power"] = CastAnim,
            ["power"] = CastAnim,
            ["PowerUp"] = CastAnim,
            ["powerup"] = CastAnim,
            ["Attack"] = AttackAnim,
            ["attack"] = AttackAnim
        };

    public static void TryPlay(NCreature? creature, string? trigger)
    {
        if (creature == null || string.IsNullOrWhiteSpace(trigger)) return;

        var sprite = FindCombatSprite(creature);
        if (sprite?.SpriteFrames == null) return;
        if (!IsOurCombatSprite(sprite)) return;

        if (IsDeathTrigger(trigger))
        {
            CombatDeathDissolve.Begin(sprite);
            return;
        }

        if (IsReviveTrigger(trigger))
            CombatDeathDissolve.Reset(sprite);

        // ディゾルブ中は Idle / 他モーションで上書きしない
        if (CombatDeathDissolve.IsDissolving(sprite))
            return;

        // 多段ヒット中のループ攻撃は、都度リスタートしない
        if (IsAttackLooping(sprite) && IsAttackTrigger(trigger))
            return;

        if (!TryMapAnim(trigger, sprite.SpriteFrames, out var anim))
            return;

        // ImmediatelySetIdle が Cast/Attack/Hurt を即上書きするのを防ぐ
        if (anim == IdleAnim && ShouldHoldCurrentAnim(sprite))
            return;

        // 単発攻撃トリガーはループOFFで再生（多段は BeginAttackLoop 側）
        if (anim == AttackAnim)
            sprite.SpriteFrames.SetAnimationLoop(AttackAnim, false);
        else if (anim == CastAnim)
            sprite.SpriteFrames.SetAnimationLoop(CastAnim, false);

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
        try
        {
            var sprite = FindOurSprite(creature);
            if (sprite?.SpriteFrames == null) return;
            if (!GodotObject.IsInstanceValid(sprite)) return;
            if (!sprite.SpriteFrames.HasAnimation(AttackAnim)) return;

            EnsureFinishedHook(sprite);
            sprite.SpriteFrames.SetAnimationLoop(AttackAnim, true);
            sprite.SetMeta(AttackLoopMeta, true);
            if (sprite.Animation != AttackAnim || !sprite.IsPlaying())
                sprite.Play(AttackAnim);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"BeginAttackLoop failed: {e.Message}");
        }
    }

    public static void EndAttackLoop(Creature? creature) =>
        EndAttackLoop(creature?.GetCreatureNode());

    public static void EndAttackLoop(NCreature? creature)
    {
        try
        {
            var sprite = FindOurSprite(creature);
            if (sprite?.SpriteFrames == null) return;
            if (!GodotObject.IsInstanceValid(sprite)) return;

            if (sprite.HasMeta(AttackLoopMeta))
                sprite.RemoveMeta(AttackLoopMeta);
            if (sprite.SpriteFrames.HasAnimation(AttackAnim))
                sprite.SpriteFrames.SetAnimationLoop(AttackAnim, false);

            if (sprite.SpriteFrames.HasAnimation(IdleAnim))
                sprite.Play(IdleAnim);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"EndAttackLoop failed: {e.Message}");
        }
    }

    /// <summary>Spine が無いとき、ゲーム側の待機時間計算用に連番アニメ長を返す。</summary>
    public static double GetCurrentAnimLengthSeconds(NCreature? creature)
    {
        var sprite = FindOurSprite(creature);
        if (sprite == null) return 0;
        if (!GodotObject.IsInstanceValid(sprite)) return 0;

        if (CombatDeathDissolve.IsDissolving(sprite))
            return CombatDeathDissolve.DurationSeconds;

        if (sprite.SpriteFrames == null) return 0;

        var anim = sprite.Animation;
        if (anim.IsEmpty || !sprite.SpriteFrames.HasAnimation(anim)) return 0;

        var frames = sprite.SpriteFrames;
        var count = frames.GetFrameCount(anim);
        var speed = frames.GetAnimationSpeed(anim);
        if (count <= 0 || speed <= 0) return 0;

        var durationSum = 0.0;
        for (var i = 0; i < count; i++)
            durationSum += frames.GetFrameDuration(anim, i);

        return durationSum / speed;
    }

    /// <summary>AnimDie が待つ残り尺（ディゾルブ中のみ。他用途へ誤って尺を渡さない）。</summary>
    public static double GetCurrentAnimTimeRemainingSeconds(NCreature? creature)
    {
        var sprite = FindOurSprite(creature);
        if (sprite == null || !GodotObject.IsInstanceValid(sprite)) return 0;

        // ShowRewards 等も同 API を呼ぶため、ディゾルブ中以外は 0 のままにする
        if (CombatDeathDissolve.IsDissolving(sprite))
            return CombatDeathDissolve.GetRemainingSeconds(sprite);

        return 0;
    }

    private static bool IsDeathTrigger(string trigger) =>
        trigger.Equals("Dead", StringComparison.OrdinalIgnoreCase)
        || trigger.Equals("Die", StringComparison.OrdinalIgnoreCase)
        || trigger.Equals("Death", StringComparison.OrdinalIgnoreCase);

    private static bool IsReviveTrigger(string trigger) =>
        trigger.Equals("Revive", StringComparison.OrdinalIgnoreCase);

    private static bool ShouldHoldCurrentAnim(AnimatedSprite2D sprite)
    {
        if (CombatDeathDissolve.IsDissolving(sprite)) return true;
        if (IsAttackLooping(sprite)) return true;
        if (!GodotObject.IsInstanceValid(sprite) || !sprite.IsPlaying()) return false;

        var current = sprite.Animation;
        if (current.IsEmpty || current == IdleAnim) return false;

        var frames = sprite.SpriteFrames;
        if (frames == null || !frames.HasAnimation(current)) return false;

        // ループしないモーション（cast / attack / hurt）再生中は Idle で潰さない
        return !frames.GetAnimationLoop(current);
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
        if (CombatDeathDissolve.IsDissolving(sprite)) return;

        var frames = sprite.SpriteFrames;
        if (frames == null) return;

        var current = sprite.Animation;
        if (current.IsEmpty) return;
        if (frames.GetAnimationLoop(current)) return;
        if (!frames.HasAnimation(IdleAnim)) return;

        sprite.Play(IdleAnim);
    }
}
