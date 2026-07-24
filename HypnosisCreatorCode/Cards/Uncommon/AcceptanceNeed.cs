using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>受容の需要 — パワー。受けた攻撃の回数を記録し、次の自ターン開始時にその数だけエナジーとドローを得る。UGでコスト1。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AcceptanceNeed() : HypnosisCreatorCard(2,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<AcceptanceNeedPower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
