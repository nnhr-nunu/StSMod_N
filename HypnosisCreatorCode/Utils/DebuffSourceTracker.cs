using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>プレイヤーへデバフを付与した敵を記録（不動明王用）。</summary>
public static class DebuffSourceTracker
{
    private static readonly NotNullSpireField<Creature, HashSet<Creature>> Field =
        new(() => []);

    public static void Record(Creature victim, Creature? applier)
    {
        if (victim == null || applier == null) return;
        if (!victim.IsPlayer || !applier.IsEnemy) return;
        Field.Get(victim).Add(applier);
    }

    public static IReadOnlyCollection<Creature> GetAppliers(Creature victim) =>
        Field.Get(victim);

    public static void Clear(Creature victim) => Field.Get(victim).Clear();
}
