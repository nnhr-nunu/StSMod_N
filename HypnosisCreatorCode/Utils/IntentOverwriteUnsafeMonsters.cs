using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// Crusher / Rocket / AdaptablePower 多段ボス（実験体など）など、
/// 意図ステート書き換え（SetMoveImmediate）や Visuals 差し替え・Attack アニメ待ちが
/// 進行不能／見た目ずれを招きやすいモンスター。
/// 睡眠は全敵共通の PerformMove スキップで扱う。ここはスタン等の1回スキップ予約用。
/// </summary>
public static class IntentOverwriteUnsafeMonsters
{
    private static readonly HashSet<string> UnsafeIds =
        new(StringComparer.OrdinalIgnoreCase) { "CRUSHER", "ROCKET" };

    private static readonly HashSet<string> KaiserClubClawIds =
        new(StringComparer.OrdinalIgnoreCase) { "CRUSHER", "ROCKET" };

    private static readonly ConditionalWeakTable<Creature, SkipState> SkipTable = new();

    public static bool IsUnsafe(Creature? creature)
    {
        if (creature is not { IsEnemy: true }) return false;
        var id = HeartRegistry.GetMonsterId(creature);
        if (id != null && UnsafeIds.Contains(id))
            return true;
        // 多段ボスは SetMoveImmediate でステートマシンが壊れやすい（実験体の進行不能等）
        return creature.GetPower<AdaptablePower>() != null;
    }

    /// <summary>見た目スライム化（Visuals 差し替え）をスキップする敵。</summary>
    public static bool SkipsVisualDisguise(Creature? creature) => IsUnsafe(creature);

    /// <summary>カイザークラブの左右爪（引き寄せ不可・専用吹き出し用）。</summary>
    public static bool IsKaiserClubClaw(Creature? creature)
    {
        if (creature is not { IsEnemy: true }) return false;
        var id = HeartRegistry.GetMonsterId(creature);
        return id != null && KaiserClubClawIds.Contains(id);
    }

    /// <summary>次の PerformMove を1回スキップする（本家 Stun の代替）。</summary>
    public static void ArmSkipOnce(Creature creature)
    {
        var state = SkipTable.GetOrCreateValue(creature);
        state.SkipCount = Math.Max(state.SkipCount, 1);
    }

    public static bool HasPendingSkip(Creature creature) =>
        SkipTable.TryGetValue(creature, out var state) && state.SkipCount > 0;

    public static bool TryConsumeSkip(Creature creature)
    {
        if (!SkipTable.TryGetValue(creature, out var state) || state.SkipCount <= 0)
            return false;
        state.SkipCount--;
        return true;
    }

    private sealed class SkipState
    {
        public int SkipCount;
    }
}
