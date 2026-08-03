using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 言葉の洪水 — パワー。催眠系カウントをプレイするたび1エナジー＋ドロー。UGでドロー2枚。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class WordFlood() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new DynamicVar("Draw", 1M)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [EnergyHoverTip];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<WordFloodPower>(
            choiceContext, Owner.Creature, DynamicVars["Draw"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars["Draw"].UpgradeValueBy(1M);
}
