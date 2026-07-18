using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>
/// 指折り数えて — カウント軸を支援するサポートカード。
/// CSV: 数える動作そのものはカウント判定に関わらないため CardKeyword.Count は付けない。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FingerCount() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1M);
}
