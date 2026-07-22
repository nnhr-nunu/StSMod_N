using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>初心者向け催眠: 次にプレイする性癖カードのタグを対象へ植え付ける。</summary>
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
        var state = Field.Get(player);
        state.Target = target;
        state.Remaining = Math.Max(0, remainingCards);

        if (state.Remaining <= 0 || player.Creature == null)
        {
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
        if (state.Remaining <= 0 || state.Target == null) return;
        if (fetishes.Count == 0) return;

        // 対象はアーム時の敵（プレイ対象と異なっても植え付け先は固定）
        var enemy = state.Target;
        if (!enemy.IsAlive)
        {
            state.Remaining = 0;
            state.Target = null;
            await ClearPlayerPower(choiceContext, player);
            return;
        }

        foreach (var fetish in fetishes.Distinct())
            FetishCombat.Awaken(enemy, fetish, player);

        state.Remaining--;
        if (state.Remaining <= 0)
            state.Target = null;

        await SyncPlayerPower(choiceContext, player, source);
    }

    private static async Task SyncPlayerPower(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel? source)
    {
        var creature = player.Creature;
        if (creature == null) return;

        var state = Field.Get(player);
        var existing = creature.GetPower<FetishPlantPendingPower>();

        if (state.Remaining <= 0)
        {
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
        public Creature? Target { get; set; }
        public int Remaining { get; set; }
    }
}
