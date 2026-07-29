using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 他色アブノーマルカード（ナイフ等）は OnPlay 内で刺さり処理しないため、AfterCardPlayed で破滅刺さりを解決する。
/// <see cref="CognitiveShuffleDeferredPatch"/> と同様、fire-and-forget ではなく Hook の Task に連鎖する。
/// 植え付け予約の消費は <see cref="Powers.FetishPlantPendingPower"/> の AfterCardPlayed で行う。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class FetishCardPlayedPatch
{
    [HarmonyPriority(Priority.First)]
    public static void Postfix(
        ref Task __result,
        ICombatState combatState,
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        // HCカードは各 OnPlay で刺さり済み
        if (play.Card is HypnosisCreatorCard) return;

        var inPool = AbnormalOtherColorPool.Contains(play.Card);
        var active = HypnosisCreatorRunRules.IsHypnosisCreatorActive(play.Card);
        if (inPool)
        {
            MainFile.Logger.Info(
                $"FetishCardPlayedPatch: {play.Card.GetType().Name} inPool={inPool} active={active} owner={play.Card.Owner?.Character?.Id.Entry}");
        }

        if (!CardFetishLookup.HasAnyFetish(play.Card)) return;

        var fetishes = CardFetishLookup.GetFetishes(play.Card);
        if (fetishes.Count == 0) return;

        var original = __result;
        __result = ResolveOtherColorFetishHitAsync(original, combatState, choiceContext, play, fetishes);
    }

    private static async Task ResolveOtherColorFetishHitAsync(
        Task? original,
        ICombatState combatState,
        PlayerChoiceContext choiceContext,
        CardPlay play,
        IReadOnlyList<FetishType> fetishes)
    {
        if (original != null)
            await original;

        var applier = play.Card.Owner?.Creature;
        if (applier == null) return;

        var target = CardFetishLookup.ResolveFetishPlayTarget(play, combatState);
        if (target != null)
        {
            await FetishCombat.TryFetishHit(
                choiceContext, target, applier, play.Card, fetishes,
                alwaysHit: false);
            return;
        }

        if (play.Card.TargetType is not (TargetType.AllEnemies or TargetType.RandomEnemy))
            return;

        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            if (!enemy.IsAlive || !enemy.IsEnemy) continue;
            await FetishCombat.TryFetishHit(
                choiceContext, enemy, applier, play.Card, fetishes,
                alwaysHit: false);
        }
    }
}
