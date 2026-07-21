using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>衰微の予兆／永劫の砂時計の CardsLeft を 6 に戻す。</summary>
public static class WitherOmen
{
    public const int DefaultCardsLeft = 6;
    public const string CardsLeftKey = "CardsLeft";

    public static void ResetOn(Creature? creature, int cardsLeft = DefaultCardsLeft)
    {
        if (creature == null) return;
        ResetPower(creature.GetPower<WitheringPresencePower>(), cardsLeft);
        ResetPower(creature.GetPower<AeonglassPower>(), cardsLeft);
    }

    public static void ResetPower(PowerModel? power, int cardsLeft = DefaultCardsLeft)
    {
        if (power == null) return;
        if (!power.DynamicVars.TryGetValue(CardsLeftKey, out var left)) return;

        left.BaseValue = cardsLeft;
    }
}
