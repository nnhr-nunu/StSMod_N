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

/// <summary>囁き — 山札の性癖カードを選び、その性癖を対象に目覚めさせる。廃棄。UGで2枚。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Whisper() : HypnosisCreatorCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    private static bool IsCandidate(CardModel c) =>
        c is HypnosisCreatorCard hc && hc.CardFetishes.Count > 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var count = DynamicVars.Cards.IntValue;
        var combat = Owner.PlayerCombatState;
        if (combat == null) return;

        var drawCandidates = combat.DrawPile.Cards
            .Where(IsCandidate)
            .ToList();

        if (drawCandidates.Count == 0) return;

        IReadOnlyList<CardModel> selected;
        try
        {
            selected = (await CardSelectCmd.FromCombatPile(
                choiceContext,
                combat.DrawPile,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, count),
                IsCandidate)).ToList();
        }
        catch
        {
            // 選択UIが使えない場合は山札からランダムに性癖を目覚めさせる
            selected = drawCandidates
                .OrderBy(_ => Guid.NewGuid())
                .Take(count)
                .ToList();
        }

        if (selected.Count == 0)
        {
            selected = drawCandidates
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
