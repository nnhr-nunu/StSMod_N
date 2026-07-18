using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 敵の攻撃意図ダメージ。本家意図UIと同じ <see cref="AttackIntent.GetTotalDamage"/> /
/// <see cref="AttackIntent.GetSingleDamage"/> を使う（筋力込み）。
/// </summary>
public static class EnemyAttackIntents
{
    public static bool IntendsToAttack(Creature enemy) =>
        enemy.Monster?.IntendsToAttack == true;

    /// <summary>意図に表示される合計ダメージ（連撃込み）。調和など。</summary>
    public static int GetTotalDamage(Creature enemy)
    {
        if (!TryGetAttackIntents(enemy, out var intents, out var targets)) return 0;

        var total = 0;
        foreach (var intent in intents)
        {
            if (intent is not AttackIntent attack) continue;
            total += Math.Max(0, attack.GetTotalDamage(targets, enemy));
        }

        return Math.Max(0, total);
    }

    /// <summary>1ヒットあたりの表示ダメージとヒット数。ミラーリングなど。</summary>
    public static bool TryGetPerHit(Creature enemy, out int perHit, out int hits)
    {
        perHit = 0;
        hits = 0;
        if (!TryGetAttackIntents(enemy, out var intents, out var targets)) return false;

        foreach (var intent in intents)
        {
            if (intent is not AttackIntent attack) continue;
            perHit = Math.Max(0, attack.GetSingleDamage(targets, enemy));
            hits = Math.Max(1, attack.Repeats);
            return true;
        }

        return false;
    }

    private static bool TryGetAttackIntents(
        Creature enemy,
        out IReadOnlyList<AbstractIntent> intents,
        out IReadOnlyList<Creature> targets)
    {
        intents = Array.Empty<AbstractIntent>();
        targets = Array.Empty<Creature>();

        var monster = enemy.Monster;
        if (monster == null || !monster.IntendsToAttack) return false;

        var move = monster.NextMove;
        if (move?.Intents == null) return false;

        var combat = enemy.CombatState;
        if (combat == null) return false;

        intents = move.Intents;
        targets = combat.PlayerCreatures;
        return true;
    }
}
