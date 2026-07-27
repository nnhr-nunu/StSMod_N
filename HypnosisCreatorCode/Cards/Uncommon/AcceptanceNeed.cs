using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>受容の需要 — パワー。HPを失った回数（攻撃・状態異常・瀉血等）を記録し、次の自ターン開始時にその数分エナジーとドロー。UGでコスト1。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AcceptanceNeed() : HypnosisCreatorCard(2,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<AcceptanceNeedPower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
