using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 敵の攻撃意図ダメージ。本家意図UIと同じ <see cref="AttackIntent.GetTotalDamage"/> /
/// <see cref="AttackIntent.GetSingleDamage"/> を使う（筋力込み）。
/// マルチでは <see cref="Player"/> 視点をカード所有者に固定する（LocalContext.GetMe 差の防止）。
/// </summary>
public static class EnemyAttackIntents
{
    public static bool IntendsToAttack(Creature enemy) =>
        enemy.Monster?.IntendsToAttack == true;

    /// <summary>意図に表示される合計ダメージ（連撃込み）。調和など。</summary>
    public static int GetTotalDamage(Creature enemy, Player? perspectivePlayer = null)
    {
        if (!TryGetAttackIntents(enemy, out var intents, out var targets)) return 0;

        using var _ = BeginPerspective(perspectivePlayer);
        var total = 0;
        foreach (var intent in intents)
        {
            if (intent is not AttackIntent attack) continue;
            total += Math.Max(0, attack.GetTotalDamage(targets, enemy));
        }

        return Math.Max(0, total);
    }

    /// <summary>1ヒットあたりの表示ダメージとヒット数。ミラーリングなど。</summary>
    public static bool TryGetPerHit(Creature enemy, out int perHit, out int hits, Player? perspectivePlayer = null)
    {
        perHit = 0;
        hits = 0;
        if (!TryGetAttackIntents(enemy, out var intents, out var targets)) return false;

        using var _ = BeginPerspective(perspectivePlayer);
        foreach (var intent in intents)
        {
            if (intent is not AttackIntent attack) continue;
            perHit = Math.Max(0, attack.GetSingleDamage(targets, enemy));
            hits = Math.Max(1, attack.Repeats);
            return true;
        }

        return false;
    }

    private static IDisposable BeginPerspective(Player? perspectivePlayer) =>
        perspectivePlayer == null
            ? NoopDisposable.Instance
            : EnemyAttackIntentPerspective.Begin(perspectivePlayer);

    private static bool TryGetAttackIntents(
        Creature enemy,
        out IReadOnlyList<AbstractIntent> intents,
        out IReadOnlyList<Creature> targets)
    {
        intents = Array.Empty<AbstractIntent>();
        targets = Array.Empty<Creature>();

        if (SleepIntentPresentation.ShouldOverride(enemy)) return false;

        var monster = enemy.Monster;
        if (monster == null || !monster.IntendsToAttack) return false;

        var move = monster.NextMove;
        if (move?.Intents == null) return false;

        var combat = enemy.CombatState;
        if (combat == null) return false;

        intents = move.Intents;
        targets = GetDeterministicPlayerTargets(combat);
        return true;
    }

    private static IReadOnlyList<Creature> GetDeterministicPlayerTargets(ICombatState combat) =>
        combat.PlayerCreatures
            .OrderBy(c => c.Player?.NetId ?? 0UL)
            .ThenBy(c => c.CombatId)
            .ToList();

    private sealed class NoopDisposable : IDisposable
    {
        public static readonly NoopDisposable Instance = new();

        public void Dispose() { }
    }
}
