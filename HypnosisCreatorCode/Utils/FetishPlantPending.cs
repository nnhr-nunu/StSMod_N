using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 初心者向け催眠: 催眠した敵へ性癖カードを使ったとき、未所持の性癖だけ植え付ける。
/// 予約は敵ごと。当たっていない敵や既に所持している性癖では消費しない。
/// 集団催眠で波及した対象もそれぞれアームする。
/// </summary>
public static class FetishPlantPending
{
    private static readonly NotNullSpireField<Player, PlantState> Field =
        new(() => new PlantState());

    public static async Task Arm(
        PlayerChoiceContext choiceContext,
        Player player,
        Creature target,
        int remainingCards,
        CardModel? source)
    {
        if (target is not { IsAlive: true, IsEnemy: true }) return;

        var state = Field.Get(player);
        PruneDead(state);

        var remaining = Math.Max(0, remainingCards);
        if (remaining <= 0)
        {
            state.RemainingByTarget.Remove(target);
            await SyncPlayerPower(choiceContext, player, source);
            return;
        }

        if (!state.RemainingByTarget.TryGetValue(target, out var current))
            current = 0;
        state.RemainingByTarget[target] = Math.Max(current, remaining);

        await SyncPlayerPower(choiceContext, player, source);
    }

    public static async Task TryConsumeOnPlay(
        PlayerChoiceContext choiceContext,
        Player player,
        CardPlay cardPlay,
        IReadOnlyList<FetishType> fetishes,
        CardModel? source)
    {
        var state = Field.Get(player);
        PruneDead(state);
        if (state.RemainingByTarget.Count == 0) return;
        if (fetishes.Count == 0) return;

        var distinct = fetishes.Distinct().ToList();
        var changed = false;
        foreach (var enemy in HitEnemies(cardPlay))
        {
            if (!state.RemainingByTarget.TryGetValue(enemy, out var remaining) || remaining <= 0)
                continue;

            var plantedAny = false;
            foreach (var fetish in distinct)
            {
                if (await FetishCombat.AwakenAsync(choiceContext, enemy, fetish, player))
                    plantedAny = true;
            }

            if (!plantedAny) continue;

            remaining--;
            if (remaining <= 0)
                state.RemainingByTarget.Remove(enemy);
            else
                state.RemainingByTarget[enemy] = remaining;
            changed = true;
        }

        if (!changed) return;

        await SyncPlayerPower(choiceContext, player, source);
    }

    private static IEnumerable<Creature> HitEnemies(CardPlay play)
    {
        var combat = play.Card.CombatState;
        if (play.Card.TargetType == TargetType.AllEnemies)
        {
            if (combat == null) yield break;
            foreach (var enemy in combat.HittableEnemies)
            {
                if (enemy is { IsAlive: true, IsEnemy: true })
                    yield return enemy;
            }

            yield break;
        }

        var target = CardFetishLookup.ResolveFetishPlayTarget(play, combat);
        if (target != null)
            yield return target;
    }

    private static void PruneDead(PlantState state)
    {
        var dead = state.RemainingByTarget.Keys
            .Where(t => t is not { IsAlive: true, IsEnemy: true })
            .ToList();
        foreach (var creature in dead)
            state.RemainingByTarget.Remove(creature);
    }

    private static int DisplayRemaining(PlantState state) =>
        state.RemainingByTarget.Count == 0 ? 0 : state.RemainingByTarget.Values.Max();

    private static async Task SyncPlayerPower(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel? source)
    {
        var creature = player.Creature;
        if (creature == null) return;

        var state = Field.Get(player);
        PruneDead(state);
        var remaining = DisplayRemaining(state);
        var existing = creature.GetPower<FetishPlantPendingPower>();

        if (remaining <= 0)
        {
            state.RemainingByTarget.Clear();
            await ClearPlayerPower(choiceContext, player);
            return;
        }

        if (existing != null)
        {
            var delta = remaining - existing.Amount;
            if (delta != 0)
            {
                await PowerCmd.ModifyAmount(
                    choiceContext, existing, delta, creature, source);
            }
            return;
        }

        await PowerCmd.Apply<FetishPlantPendingPower>(
            choiceContext, creature, remaining, creature, source);
    }

    private static async Task ClearPlayerPower(PlayerChoiceContext choiceContext, Player player)
    {
        _ = choiceContext;
        var existing = player.Creature?.GetPower<FetishPlantPendingPower>();
        if (existing != null)
            await PowerCmd.Remove(existing);
    }

    private sealed class PlantState
    {
        public Dictionary<Creature, int> RemainingByTarget { get; } = [];
    }
}
