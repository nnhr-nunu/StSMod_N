using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>性癖が刺さりうるときカードを黄色ハイライトする判定。</summary>
public static class FetishGlow
{
    public static bool ShouldGlow(CardModel card)
    {
        var fetishes = CardFetishLookup.GetFetishes(card);
        var alwaysHit = CardFetishLookup.AlwaysHitsFetish(card);
        if (fetishes.Count == 0 && !alwaysHit) return false;

        var combat = card.CombatState;
        if (combat == null) return false;

        foreach (var enemy in ResolveCandidates(card, combat.HittableEnemies))
        {
            if (!enemy.IsAlive || !enemy.IsEnemy) continue;
            if (WouldHit(card, enemy, fetishes, alwaysHit)) return true;
        }

        return false;
    }

    private static IEnumerable<Creature> ResolveCandidates(
        CardModel card,
        IReadOnlyList<Creature> hittable)
    {
        if (card.CurrentTarget != null)
            return [card.CurrentTarget];

        return hittable;
    }

    private static bool WouldHit(
        CardModel card,
        Creature target,
        IReadOnlyList<FetishType> fetishes,
        bool alwaysHit)
    {
        if (alwaysHit && fetishes.Count > 0) return true;

        foreach (var fetish in fetishes.Distinct())
        {
            if (FetishCombat.HasFetish(target, fetish)) return true;
            if (FetishCombat.CultLeaderActive && fetish != FetishType.Trance) return true;
        }

        return false;
    }
}
