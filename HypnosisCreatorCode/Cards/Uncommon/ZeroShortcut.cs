using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// ゼロショートカット — 手札のカウントカード1枚のコストをこの戦闘中0にする。
/// CSV: 支援カードのため CardKeyword.Count は付けない。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class ZeroShortcut() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    private static bool IsCandidate(CardModel c) =>
        c.Keywords.Contains(HcKeywords.Count) && c.EnergyCost.GetResolved() > 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combat = Owner.PlayerCombatState;
        var hand = combat?.Hand;
        if (hand == null) return;

        var candidates = hand.Cards.Where(IsCandidate).ToList();
        if (candidates.Count == 0) return;

        IReadOnlyList<CardModel> selected;
        try
        {
            selected = (await CardSelectCmd.FromCombatPile(
                choiceContext,
                hand,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1),
                IsCandidate)).ToList();
        }
        catch
        {
            selected = [candidates[Owner.RunState.Rng.CombatCardSelection.NextInt(candidates.Count)]];
        }

        foreach (var card in selected)
            card.EnergyCost.AddThisCombat(-card.EnergyCost.GetResolved());
    }
}
