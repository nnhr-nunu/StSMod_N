using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 言葉の洪水 — 催眠系カウントカードをプレイするたび、1エナジーを得て Amount 枚ドロー。
/// </summary>
public class WordFloodPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (MassHypnosisPower.IsPropagating) return;
        if (!CountRules.HasCountKeyword(cardPlay.Card)) return;

        var player = Owner.Player;
        if (player == null) return;

        await PlayerCmd.GainEnergy(1, player);
        await CardPileCmd.Draw(choiceContext, Amount, player);
    }
}
