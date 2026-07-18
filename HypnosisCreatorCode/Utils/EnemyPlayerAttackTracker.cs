using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>戦闘中、各敵がプレイヤーを攻撃した回数（お仕置き等）。</summary>
public static class EnemyPlayerAttackTracker
{
    private static readonly Dictionary<Creature, int> Counts = new();

    public static void Record(Creature enemy)
    {
        if (!enemy.IsEnemy) return;
        Counts[enemy] = GetCount(enemy) + 1;
    }

    public static int GetCount(Creature enemy) =>
        Counts.TryGetValue(enemy, out var count) ? count : 0;

    public static void Reset() => Counts.Clear();
}
