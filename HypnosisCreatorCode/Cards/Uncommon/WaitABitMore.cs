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

    protected override bool ShouldGlowWhenConditionMet()
    {
        var hand = Owner.PlayerCombatState?.Hand;
        return hand != null && hand.Cards.Any(c => c != this && CountRules.HasCountKeyword(c));
    }

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
        // 選択できない文脈（ThrowingPlayerChoiceContext）だけ代替する。これは全端末で同じく起きる。
        // 画面の取り消し等は選ぶ本人の端末でしか起きないため握りつぶさない（片方だけ代替が走ると共闘で同期ずれ）。
        catch (NotImplementedException)
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
