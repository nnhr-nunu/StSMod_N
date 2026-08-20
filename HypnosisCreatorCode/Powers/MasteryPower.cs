using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 練達 — このパワーが有効な間にプレイしたカウントカードを、戦闘終了時に永続的にアップグレードする。
/// UG版は戦闘終了後に催眠系カウントカード報酬を追加する（本家 Royalties / ForbiddenGrimoire 同型）。
/// </summary>
public class MasteryPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool AddHypnosisCountCardReward { get; set; }

    private readonly HashSet<CardModel> _playedCountCards = [];
    public IReadOnlyCollection<CardModel> PlayedCountCards => _playedCountCards;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || cardPlay.Card.Owner?.Creature != Owner) return Task.CompletedTask;
        // 波及コピーは一時カード。デッキ実体の追跡に混ぜない。
        if (MassHypnosisPower.IsPropagating) return Task.CompletedTask;
        if (!CountRules.HasCountKeyword(cardPlay.Card)) return Task.CompletedTask;

        var persistent = cardPlay.Card.DeckVersion ?? cardPlay.Card;
        if (persistent.IsDupe) return Task.CompletedTask;

        _playedCountCards.Add(persistent);
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        var player = Owner?.Player;
        if (player == null) return Task.CompletedTask;

        foreach (var card in _playedCountCards.ToList())
        {
            if (!card.IsUpgradable) continue;
            CardCmd.Upgrade(card, CardPreviewStyle.None);
        }

        if (AddHypnosisCountCardReward)
        {
            var cards = HypnosisCountCardPool.Roll(player, 3);
            if (cards.Count > 0)
            {
                var rerollOptions = CardCreationOptions.ForRoom(player, room.RoomType);
                var reward = new CardReward(cards, CardCreationSource.Other, player, rerollOptions)
                {
                    CanReroll = false
                };
                room.AddExtraReward(player, reward);
            }
        }

        return Task.CompletedTask;
    }
}
