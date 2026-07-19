using HypnosisCreator.HypnosisCreatorCode.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>性癖が刺さりうるときカードを黄色ハイライトする判定。</summary>
public static class FetishGlow
{
    public static bool ShouldGlow(CardModel card)
    {
        if (card is not HypnosisCreatorCard hc) return false;
        if (hc.CardFetishes.Count == 0 && !hc.AlwaysHitsFetish) return false;

        var combat = hc.CombatState;
        if (combat == null) return false;

        foreach (var enemy in ResolveCandidates(hc, combat.HittableEnemies))
        {
            if (!enemy.IsAlive || !enemy.IsEnemy) continue;
            if (WouldHit(hc, enemy)) return true;
        }

        return false;
    }

    private static IEnumerable<Creature> ResolveCandidates(
        HypnosisCreatorCard card,
        IReadOnlyList<Creature> hittable)
    {
        if (card.CurrentTarget != null)
            return [card.CurrentTarget];

        // 手札時点でも「誰かに刺さる」なら光らせる（AoE・単体とも）
        return hittable;
    }

    private static bool WouldHit(HypnosisCreatorCard card, Creature target)
    {
        // タグ付き必中（感度3000倍等）は常に刺さるので光る。
        // タグ無し必中（囁きUG等）はカード固有の条件ハイライトに任せる。
        if (card.AlwaysHitsFetish && card.CardFetishes.Count > 0) return true;

        foreach (var fetish in card.CardFetishes.Distinct())
        {
            if (FetishCombat.HasFetish(target, fetish)) return true;
            if (FetishCombat.CultLeaderActive && fetish != FetishType.Trance) return true;
        }

        return false;
    }
}
