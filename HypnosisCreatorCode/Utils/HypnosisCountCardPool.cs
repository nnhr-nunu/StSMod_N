using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>練達UG報酬など、催眠系カウントカードの抽選プール。</summary>
public static class HypnosisCountCardPool
{
    public static bool IsEligible(CardModel card) =>
        card is HypnosisCreatorCard { Rarity: not CardRarity.Token }
        && CountRules.HasCountKeyword(card)
        && card is not TrainingCommand;

    public static List<CardModel> Roll(Player player, int count)
    {
        var pool = ModelDb.AllCards.Where(IsEligible).ToList();
        if (pool.Count == 0) return [];

        var rng = player.RunState.Rng.CombatCardSelection;
        return pool
            .OrderBy(_ => rng.NextInt(int.MaxValue))
            .Take(count)
            .Select(card => card.CreateCloneForPlayer(player))
            .ToList();
    }
}
