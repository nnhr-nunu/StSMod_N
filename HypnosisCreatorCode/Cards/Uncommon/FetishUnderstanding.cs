using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>性倒錯への理解 — パワー。性癖タグ付きカードをプレイするたびブロックを得る。UGでブロック量が増加する。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FetishUnderstanding() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Block", 2M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<FetishUnderstandingPower>(
            choiceContext, Owner.Creature, DynamicVars["Block"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars["Block"].UpgradeValueBy(1M);
}
