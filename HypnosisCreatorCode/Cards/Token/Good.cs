using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>Good! — No.66。廃棄されたカード1枚を選び手札に加える。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Good() : TrainingCommand(TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var exhaust = Owner.PlayerCombatState?.ExhaustPile;
        if (exhaust == null || exhaust.Cards.Count == 0) return;

        IReadOnlyList<CardModel> selected;
        try
        {
            selected = (await CardSelectCmd.FromCombatPile(
                choiceContext, exhaust, Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1))).ToList();
        }
        catch
        {
            var rng = Owner.RunState.Rng.CombatCardSelection;
            selected = [exhaust.Cards[rng.NextInt(exhaust.Cards.Count)]];
        }

        foreach (var card in selected)
            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Top, this);
    }
}
