using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>
/// 指折り数えて — 再生3を得る。手札のカウントカードすべてのコストを1下げる。廃棄。UGでコスト0。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FingerCount() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<RegenPower>(3M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<RegenPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;
        await PowerCmd.Apply<RegenPower>(
            choiceContext, self, DynamicVars["RegenPower"].BaseValue, self, this);
        CountRules.AdvanceHandCountCards(Owner);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
