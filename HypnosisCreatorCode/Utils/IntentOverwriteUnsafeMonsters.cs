using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// Crusher / Rocket など、意図ステート書き換え（SetMoveImmediate）や
/// 本体 Attack アニメ待ちが進行不能を招きやすいモンスター。
/// 見た目が戦闘背景一体の敵向け。行動ルール自体は通常敵と同じに保つ。
/// </summary>
public static class IntentOverwriteUnsafeMonsters
{
    private static readonly HashSet<string> UnsafeIds =
        new(StringComparer.OrdinalIgnoreCase) { "CRUSHER", "ROCKET" };

    private static readonly ConditionalWeakTable<Creature, SkipState> SkipTable = new();

    public static bool IsUnsafe(Creature? creature)
    {
        if (creature is not { IsEnemy: true }) return false;
        var id = HeartRegistry.GetMonsterId(creature);
        return id != null && UnsafeIds.Contains(id);
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
