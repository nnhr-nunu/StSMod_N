using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 状態異常催眠：トランス状態の敵へ、敵対象の状態異常・呪いをプレイ可能にする。
/// <see cref="PlayableStatusCard.FreeEnemyPlay"/>（心臓付与）は催眠不要。
/// </summary>
public static class StatusHypnosisRules
{
    public static bool OwnerHasStatusHypnosis(CardModel card) =>
        card.Owner?.Creature?.GetPower<StatusHypnosisPower>() != null;

    public static bool IsEnemyTargetedStatusOrCurse(CardModel card) =>
        card.Type is CardType.Status or CardType.Curse
        && card.TargetType is TargetType.AnyEnemy or TargetType.AllEnemies or TargetType.RandomEnemy;

    public static bool IsGated(CardModel card)
    {
        if (card is PlayableStatusCard { FreeEnemyPlay: true })
            return false;
        return IsEnemyTargetedStatusOrCurse(card);
    }

    public static bool CanStartPlay(CardModel card)
    {
        if (card is PlayableStatusCard { FreeEnemyPlay: true })
        {
            var combat = card.CombatState;
            return combat != null && combat.HittableEnemies.Count > 0;
        }

        if (!IsGated(card)) return true;
        if (!OwnerHasStatusHypnosis(card)) return false;

        var state = card.CombatState;
        if (state == null) return false;
        return TranceCombat.AnyEnemyHasTrance(state.HittableEnemies);
    }

    public static bool CanPlayTargeting(CardModel card, Creature target)
    {
        if (card is PlayableStatusCard { FreeEnemyPlay: true })
            return target.IsAlive && target.IsEnemy;

        if (!IsGated(card)) return true;
        if (!OwnerHasStatusHypnosis(card)) return false;

        return card.TargetType switch
        {
            TargetType.AnyEnemy => TranceCombat.HasTrance(target),
            TargetType.AllEnemies or TargetType.RandomEnemy =>
                card.CombatState != null &&
                TranceCombat.AnyEnemyHasTrance(card.CombatState.HittableEnemies),
            _ => true
        };
    }
}
