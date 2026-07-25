using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 触手の想起 — プレイしたターンの終了後に予約し、次の自ターン開始時に
/// 本家「怒り」同様 <see cref="CardModel.CreateClone"/> でコピーを手札へ加える。
/// </summary>
public class TentacleRecallPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private CardModel? _source;
    private bool _deliverNextTurn;

    public void Schedule(CardModel source)
    {
        _source = source;
        _deliverNextTurn = false;
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Owner == null || _source == null) return Task.CompletedTask;
        if (!participants.Contains(Owner)) return Task.CompletedTask;

        _deliverNextTurn = true;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!_deliverNextTurn || Owner == null || player.Creature != Owner || _source == null)
            return;

        _deliverNextTurn = false;
        var copy = _source.CreateClone();
        var result = await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, player);
        CardCmd.PreviewCardPileAdd(result);
        await PowerCmd.Remove(this);
    }
}
