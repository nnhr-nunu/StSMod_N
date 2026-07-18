using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>言葉の洪水 — カウントカードをプレイするたび、Amount 枚のカードを引く。</summary>
public class WordFloodPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (!CountRules.HasCountKeyword(cardPlay.Card)) return;

        var player = Owner.Player;
        if (player == null) return;

        await CardPileCmd.Draw(choiceContext, Amount, player);
    }
}
