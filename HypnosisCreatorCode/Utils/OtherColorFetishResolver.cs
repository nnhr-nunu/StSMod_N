using HypnosisCreator.HypnosisCreatorCode.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 他色アブノーマルカード（ナイフ等）の性癖刺さり。Hook.AfterCardPlayed 直後に解決する。
/// </summary>
internal static class OtherColorFetishResolver
{
    public static async Task TryResolveAfterCardPlayedAsync(CardPlay cardPlay)
    {
        if (cardPlay.Card is HypnosisCreatorCard) return;
        if (!CardFetishLookup.HasAnyFetish(cardPlay.Card)) return;

        var fetishes = CardFetishLookup.GetFetishes(cardPlay.Card);
        if (fetishes.Count == 0) return;

        var combatState = cardPlay.Card.CombatState;
        if (combatState == null) return;

        var applier = cardPlay.Card.Owner?.Creature;
        if (applier == null) return;

        var target = CardFetishLookup.ResolveFetishPlayTarget(cardPlay, combatState);
        if (target != null)
        {
            await FetishCombat.TryFetishHit(
                new ThrowingPlayerChoiceContext(), target, applier, cardPlay.Card, fetishes,
                alwaysHit: false);
            return;
        }

        if (cardPlay.Card.TargetType is not (TargetType.AllEnemies or TargetType.RandomEnemy))
            return;

        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            if (!enemy.IsAlive || !enemy.IsEnemy) continue;
            await FetishCombat.TryFetishHit(
                new ThrowingPlayerChoiceContext(), enemy, applier, cardPlay.Card, fetishes,
                alwaysHit: false);
        }
    }
}
