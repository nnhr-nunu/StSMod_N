using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>初心者向け催眠: 次にプレイする性癖カードのタグを対象へ植え付ける。</summary>
public static class FetishPlantPending
{
    private static readonly NotNullSpireField<Player, PlantState> Field =
        new(() => new PlantState());

    public static void Arm(Player player, Creature target, int remainingCards)
    {
        var state = Field.Get(player);
        state.Target = target;
        state.Remaining = Math.Max(0, remainingCards);
    }

    public static async Task TryConsumeOnPlay(
        Player player,
        Creature? playTarget,
        IReadOnlyList<FetishType> fetishes)
    {
        var state = Field.Get(player);
        if (state.Remaining <= 0 || state.Target == null) return;
        if (fetishes.Count == 0) return;
        // 対象はアーム時の敵（プレイ対象と異なっても植え付け先は固定）
        var enemy = state.Target;
        if (!enemy.IsAlive) 
        {
            state.Remaining = 0;
            state.Target = null;
            return;
        }

        foreach (var fetish in fetishes.Distinct())
            FetishCombat.Awaken(enemy, fetish, player);

        state.Remaining--;
        if (state.Remaining <= 0)
            state.Target = null;

        await Task.CompletedTask;
    }

    private sealed class PlantState
    {
        public Creature? Target { get; set; }
        public int Remaining { get; set; }
    }
}
