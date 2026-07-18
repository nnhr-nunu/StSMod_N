using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>トランス付与回数の累計（トランスに溶けゆく用）。</summary>
public static class TranceFallTracker
{
    private static readonly NotNullSpireField<Creature, TranceFallState> Field =
        new(() => new TranceFallState());

    public static int Get(Creature enemy) => Field.Get(enemy).TotalApplied;

    public static void Add(Creature enemy, int amount)
    {
        if (!enemy.IsEnemy || amount <= 0) return;
        Field.Get(enemy).TotalApplied += amount;
    }

    public static void Reset(Creature enemy) => Field.Get(enemy).TotalApplied = 0;
}

public sealed class TranceFallState
{
    public int TotalApplied { get; set; }
}
