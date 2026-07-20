using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 深淵からの声援 UG — 次の自ターン開始時、登録カードをあらゆる場所から手札へ加える。
/// 回収方法は本家 顕現（SummonForth）の AllCards → Hand と同じ。
/// </summary>
public class AbyssCheerReturnPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private readonly HashSet<CardModel> _cards = [];

    public void Schedule(CardModel card)
    {
        if (card != null)
            _cards.Add(card);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player.Creature != Owner) return;

        var pending = _cards.Where(c => c.Owner == player && c.Pile is { IsCombatPile: true }).ToList();
        _cards.Clear();

        var toHand = pending
            .Where(c => c.Pile?.Type != PileType.Hand)
            .ToList();

        if (toHand.Count > 0)
        {
            Flash();
            await CardPileCmd.Add(toHand, PileType.Hand, CardPilePosition.Top, this);
        }

        await PowerCmd.Remove(this);
    }
}
