using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 教祖化 — パワー。有効な間、SM・DomSub・アブノーマルの性癖カードは必ず刺さる（トランス性癖は対象外）。
/// トランス付与時、次のターンのエナジー+1・ドロー+Amount を得る。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class CultLeader() : HypnosisCreatorCard(3,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Draw", 1M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<CultLeaderPower>(
            choiceContext, Owner.Creature, DynamicVars["Draw"].BaseValue, Owner.Creature, this);

    protected override void OnUpgrade() => DynamicVars["Draw"].UpgradeValueBy(1M);
}
