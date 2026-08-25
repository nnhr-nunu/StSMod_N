using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 練達 — このパワーが有効な間にプレイしたカウントカードを、戦闘終了時に永続的にアップグレードする。
/// 神格化（Apotheosis）等の戦闘コピーUGは一時的なので、デッキ実体（DeckVersion）は別途ここで永続UGする。
/// 糸色丁頁など MaxUpgradeLevel が複数のカードは IsUpgradable のまま追加UGする。
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

        // 神格化は戦闘コピーだけをUGする。永続化対象は常にデッキ実体。
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
            try
            {
                TryUpgradePersistent(card);
            }
            catch (Exception e)
            {
                // AfterCombatEnd を例外で中断すると戦闘終了自体が止まる。
                MainFile.Logger.Warn(
                    $"MasteryPower: upgrade failed for {card?.Id.Entry}: {e.Message}");
            }
        }

        if (AddHypnosisCountCardReward)
        {
            try
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
            catch (Exception e)
            {
                MainFile.Logger.Warn($"MasteryPower: count card reward failed: {e.Message}");
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 戦闘終了中は CardCmd.Upgrade の履歴／演出経路を避け、デッキ実体へ無音で永続UGする。
    /// IsUpgradable により通常カードは未UGのみ、糸色丁頁など無限UGは追加UGする。
    /// </summary>
    private static void TryUpgradePersistent(CardModel card)
    {
        if (!card.IsUpgradable) return;

        // ラン履歴はベストエフォート（GetEntry 失敗で戦闘終了を落とさない）
        if (card.Pile?.Type == PileType.Deck && card.Owner != null)
        {
            try
            {
                var history = card.Owner.RunState.CurrentMapPointHistoryEntry;
                history?.GetEntry(card.Owner.NetId).UpgradedCards.Add(card.Id);
            }
            catch (Exception e)
            {
                MainFile.Logger.Warn($"MasteryPower: upgrade history skipped: {e.Message}");
            }
        }

        // CardCmd.Upgrade は IsEnding 早期 return・履歴例外・VFX 経路がある。
        // AfterCombatEnd では本家 Improvement と同様に永続UGだけ確実に通す。
        card.UpgradeInternal();
        card.FinalizeUpgradeInternal();
    }
}
