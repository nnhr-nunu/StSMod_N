using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>触手の想起 — 次の自ターン開始時、このカードのコピーを1枚手札に加えて消滅する。</summary>
public class TentacleRecallPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public CardModel? SourceCard { get; set; }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player.Creature != Owner || SourceCard == null || CombatState == null) return;

        var copy = CombatState.CreateCard(SourceCard, player);
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, player);
        await PowerCmd.Remove(this);
    }
}
