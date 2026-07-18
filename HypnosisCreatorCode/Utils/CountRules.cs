using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>カウントのプレイ可否。mechanics-lock.md 参照。</summary>
public static class CountRules
{
    public static bool HasCountKeyword(CardModel card) =>
        card.Keywords.Contains(HcKeywords.Count);

    public static bool IsResolvedCostZero(CardModel card) =>
        card.EnergyCost.GetResolved() == 0;

    /// <summary>手札から掴めるか（単体は「誰かがトランス」で可）。</summary>
    public static bool CanStartPlay(CardModel card)
    {
        if (!HasCountKeyword(card)) return true;
        if (IsResolvedCostZero(card)) return true;

        var combat = card.CombatState;
        if (combat == null) return false;

        return card.TargetType switch
        {
            TargetType.AllEnemies or TargetType.RandomEnemy =>
                TranceCombat.AnyEnemyHasTrance(combat.HittableEnemies),
            TargetType.AnyEnemy =>
                TranceCombat.AnyEnemyHasTrance(combat.HittableEnemies),
            _ => false // Self / None 等: トランス解除なし
        };
    }

    /// <summary>着弾対象が有効か。</summary>
    public static bool CanPlayTargeting(CardModel card, Creature target)
    {
        if (!HasCountKeyword(card)) return true;
        if (IsResolvedCostZero(card)) return true;

        return card.TargetType switch
        {
            TargetType.AnyEnemy => TranceCombat.HasTrance(target),
            TargetType.AllEnemies or TargetType.RandomEnemy =>
                card.CombatState != null &&
                TranceCombat.AnyEnemyHasTrance(card.CombatState.HittableEnemies),
            _ => false
        };
    }
}
