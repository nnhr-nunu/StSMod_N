using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 初心者向け催眠: 次にプレイする性癖カードのタグを、アーム済みの敵へ植え付ける。
/// 集団催眠で波及した場合は対象を複数保持し、1枚の性癖カードで全員に植え付ける。
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

        if (!state.Targets.Contains(target))
            state.Targets.Add(target);

        // 波及コピーは同じ PlantCards。枚数は Max で揃え、対象だけ増やす。
        state.Remaining = Math.Max(state.Remaining, Math.Max(0, remainingCards));

        if (state.Remaining <= 0 || state.Targets.Count == 0 || player.Creature == null)
        {
            state.Targets.Clear();
            state.Remaining = 0;
            await ClearPlayerPower(choiceContext, player);
            return;
        }

        await SyncPlayerPower(choiceContext, player, source);
    }

    public static async Task TryConsumeOnPlay(
        PlayerChoiceContext choiceContext,
        Player player,
        Creature? playTarget,
        IReadOnlyList<FetishType> fetishes,
        CardModel? source)
    {
        _ = playTarget;
        var state = Field.Get(player);
        PruneDead(state);
        if (state.Remaining <= 0 || state.Targets.Count == 0) return;
        if (fetishes.Count == 0) return;

        var distinct = fetishes.Distinct().ToList();
        foreach (var enemy in state.Targets.ToList())
        {
            if (!enemy.IsAlive) continue;
            foreach (var fetish in distinct)
                FetishCombat.Awaken(enemy, fetish, player);
        }

        state.Remaining--;
        if (state.Remaining <= 0)
            state.Targets.Clear();

        await SyncPlayerPower(choiceContext, player, source);
    }

    private static void PruneDead(PlantState state) =>
        state.Targets.RemoveAll(t => t is not { IsAlive: true, IsEnemy: true });

    private static async Task SyncPlayerPower(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel? source)
    {
        var creature = player.Creature;
        if (creature == null) return;

        var state = Field.Get(player);
        PruneDead(state);
        var existing = creature.GetPower<FetishPlantPendingPower>();

        if (state.Remaining <= 0 || state.Targets.Count == 0)
        {
            state.Remaining = 0;
            state.Targets.Clear();
            await ClearPlayerPower(choiceContext, player);
            return;
        }

        if (existing != null)
        {
            var delta = state.Remaining - existing.Amount;
            if (delta != 0)
            {
                await PowerCmd.ModifyAmount(
                    choiceContext, existing, delta, creature, source);
            }
            return;
        }

        await PowerCmd.Apply<FetishPlantPendingPower>(
            choiceContext, creature, state.Remaining, creature, source);
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
        public List<Creature> Targets { get; } = [];
        public int Remaining { get; set; }
    }
}
