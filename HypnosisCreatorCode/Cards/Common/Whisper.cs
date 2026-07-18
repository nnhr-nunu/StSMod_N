using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>囁き — 手札の性癖カードを選び、その性癖を対象に目覚めさせる。UGで2枚。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Whisper() : HypnosisCreatorCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var count = DynamicVars.Cards.IntValue;
        var combat = Owner.PlayerCombatState;
        if (combat == null) return;

        var handCandidates = combat.Hand.Cards
            .Where(c => c != this && c is HypnosisCreatorCard hc && hc.CardFetishes.Count > 0)
            .ToList();

        if (handCandidates.Count == 0) return;

        IReadOnlyList<CardModel> selected;
        try
        {
            selected = (await CardSelectCmd.FromHand(
                choiceContext,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, count),
                c => c != this && c is HypnosisCreatorCard hc && hc.CardFetishes.Count > 0,
                this)).ToList();
        }
        catch
        {
            // 選択UIが使えない場合は手札からランダムに性癖を目覚めさせる
            selected = handCandidates
                .OrderBy(_ => Guid.NewGuid())
                .Take(count)
                .ToList();
        }

        if (selected.Count == 0)
        {
            selected = handCandidates
                .OrderBy(_ => Guid.NewGuid())
                .Take(count)
                .ToList();
        }

        foreach (var card in selected)
        {
            if (card is not HypnosisCreatorCard hc) continue;
            foreach (var fetish in hc.CardFetishes)
                FetishCombat.Awaken(play.Target, fetish, Owner);
        }

        await Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1M);
}
