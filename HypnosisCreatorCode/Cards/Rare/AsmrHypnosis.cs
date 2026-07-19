using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// ASMR催眠 — マルチ用パワー・コスト2。Left/Right 交互プレイで破滅5（UG8）。
/// ソロでも自己交互で動作する。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AsmrHypnosis() : HypnosisCreatorCard(2,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Doom", 5M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<AsmrHypnosisPower>(
            choiceContext, Owner.Creature, DynamicVars["Doom"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars["Doom"].UpgradeValueBy(3M);
}
