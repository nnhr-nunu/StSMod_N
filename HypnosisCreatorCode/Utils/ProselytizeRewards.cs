using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>布教欲求の戦闘終了ゴールドをプレイヤー単位で溜める。</summary>
public static class ProselytizeRewards
{
    private sealed class GoldState
    {
        public decimal Amount;
    }

    private static readonly NotNullSpireField<Creature, GoldState> GoldField = new(() => new GoldState());

    public static void AddGold(Creature playerCreature, decimal amount)
    {
        if (playerCreature == null || amount <= 0M) return;
        GoldField.Get(playerCreature).Amount += amount;
    }

    public static decimal TakeGold(Creature playerCreature)
    {
        if (playerCreature == null) return 0M;
        var state = GoldField.Get(playerCreature);
        var amount = state.Amount;
        state.Amount = 0M;
        return amount;
    }
}
