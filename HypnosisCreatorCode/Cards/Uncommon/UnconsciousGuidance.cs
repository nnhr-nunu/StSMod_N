using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>無意識下の誘導 — 敵の攻撃ヒット前にブロックを得る。UGでブロック量増加。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class UnconsciousGuidance() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Block", 3M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<UnconsciousGuidancePower>(
            choiceContext, Owner.Creature, DynamicVars["Block"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars["Block"].UpgradeValueBy(1M);
}
