using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// アンコール — プレイしたカウントカードの行き先を廃棄から手札へ差し替え、コストを初期値へ戻す。
/// OnPlayWrapper は AfterCardPlayed のあとで Exhaust するため、廃棄済み判定付きの
/// AfterCardPlayed では手遅れ。本家 Rebound と同様に ModifyCardPlayResultLocation で差し替える。
/// </summary>
public class EncorePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override CardLocation ModifyCardPlayResultLocation(
        CardModel card,
        bool wasAutoPlay,
        ResourceInfo resourcesSpent,
        CardLocation location)
    {
        if (Owner == null || card.Owner?.Creature != Owner)
            return location;
        if (!CountRules.HasCountKeyword(card))
            return location;
        if (location.pileType != PileType.Exhaust)
            return location;

        return new CardLocation(card.Owner, PileType.Hand, CardPilePosition.Top);
    }

    public override Task AfterModifyingCardPlayResultLocation(CardModel card, CardLocation location)
    {
        if (Owner == null || card.Owner?.Creature != Owner)
            return Task.CompletedTask;
        if (!CountRules.HasCountKeyword(card))
            return Task.CompletedTask;
        if (location.pileType != PileType.Hand)
            return Task.CompletedTask;

        Flash();
        card.EnergyCost.SetThisCombat(card.EnergyCost.Canonical);
        return Task.CompletedTask;
    }
}
