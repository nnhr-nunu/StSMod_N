using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>
/// 指折り数えて — 手札のカウントカードすべてのコストを1下げる。UGでコスト0。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FingerCount() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CountRules.AdvanceHandCountCards(Owner);
        await Task.CompletedTask;
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
