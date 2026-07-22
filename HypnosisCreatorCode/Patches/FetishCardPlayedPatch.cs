using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 他色アブノーマルカードは OnPlay 内で刺さり処理しないため、AfterCardPlayed で破滅刺さりを解決する。
/// 植え付け予約の消費は <see cref="Powers.FetishPlantPendingPower"/> の AfterCardPlayed で行う
/// （Harmony Postfix から PowerCmd を GetResult すると CustomScaledWait で詰まる）。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class FetishCardPlayedPatch
{
    public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (!CardFetishLookup.HasAnyFetish(play.Card)) return;

        // HCカードは各 OnPlay で刺さり済み
        if (play.Card is HypnosisCreatorCard) return;

        var fetishes = CardFetishLookup.GetFetishes(play.Card);
        if (fetishes.Count == 0) return;

        _ = ResolveOtherColorFetishHit(combatState, choiceContext, play, fetishes);
    }

    private static async Task ResolveOtherColorFetishHit(
        ICombatState combatState,
        PlayerChoiceContext choiceContext,
        CardPlay play,
        IReadOnlyList<FetishType> fetishes)
    {
        var applier = play.Card.Owner?.Creature;
        if (applier == null) return;

        if (play.Target is { IsEnemy: true, IsAlive: true })
        {
            await FetishCombat.TryFetishHit(
                choiceContext, play.Target, applier, play.Card, fetishes,
                alwaysHit: false, singleHit: true);
            return;
        }

        if (play.Card.TargetType is not (TargetType.AllEnemies or TargetType.RandomEnemy))
            return;

        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            if (!enemy.IsAlive || !enemy.IsEnemy) continue;
            await FetishCombat.TryFetishHit(
                choiceContext, enemy, applier, play.Card, fetishes,
                alwaysHit: false, singleHit: true);
        }
    }
}
