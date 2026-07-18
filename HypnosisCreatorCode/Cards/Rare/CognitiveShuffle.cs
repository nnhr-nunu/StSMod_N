using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 思考シャッフル — CSV: 手札・山札を混ぜ合わせる複雑な並び替え演出を想定。
/// 詳細仕様未確定のため、暫定でカードを多めに引く効果に差し替え。
/// TODO: 本来の「シャッフル」演出仕様が確定したら差し替える。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CognitiveShuffle() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(3)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1M);
}
