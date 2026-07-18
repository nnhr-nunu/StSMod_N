using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>トランス付与・判定。仕様は mechanics-lock.md。</summary>
public static class TranceCombat
{
    public static int GetTrance(Creature creature) =>
        creature.GetPowerAmount<TrancePower>();

    public static bool HasTrance(Creature creature) => GetTrance(creature) > 0;

    public static bool AnyEnemyHasTrance(IEnumerable<Creature> enemies) =>
        enemies.Any(e => e.IsAlive && HasTrance(e));

    public static async Task ApplyTrance(
        PlayerChoiceContext choiceContext,
        Creature target,
        int amount,
        Creature applier,
        CardModel? cardSource = null)
    {
        if (amount <= 0 || !target.IsEnemy) return;
        await PowerCmd.Apply<TrancePower>(choiceContext, target, amount, applier, cardSource!);

        // トランスに溶けゆく用: 付与回数を累計
        TranceFallTracker.Add(target, amount);
    }
}
