using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>もうちょっと我慢しようね — 手札のカウントカードを選んでコストを基本値に戻し、リプレイ1を付与する。廃棄。UGで2枚選択。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class WaitABitMore() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = Owner.PlayerCombatState?.Hand;
        if (hand == null) return;

        var candidates = hand.Cards.Where(c => c != this && CountRules.HasCountKeyword(c)).ToList();
        if (candidates.Count == 0) return;

        var count = Math.Min(DynamicVars.Cards.IntValue, candidates.Count);
        IReadOnlyList<CardModel> selected;
        try
        {
            selected = (await CardSelectCmd.FromCombatPile(
                choiceContext, hand, Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, count),
                c => c != this && CountRules.HasCountKeyword(c))).ToList();
        }
        catch
        {
            selected = candidates.Take(count).ToList();
        }

        foreach (var card in selected)
        {
            card.EnergyCost.SetThisCombat(card.EnergyCost.Canonical);
            card.BaseReplayCount = 1;
        }
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1M);
}
