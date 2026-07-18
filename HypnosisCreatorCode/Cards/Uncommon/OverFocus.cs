using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>過集中 — 手札スキル1枚につきエナジー1。廃棄。UGで廃棄なし。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class OverFocus() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = Owner.PlayerCombatState?.Hand;
        if (hand == null) return;

        var skillCount = hand.Cards.Count(c => c.Type == CardType.Skill);
        if (skillCount > 0)
            await PlayerCmd.GainEnergy(skillCount, Owner);
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
