using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>アンコール — パワー。プレイしたカウントカードが廃棄される代わりに手札へ戻る。UGでコスト0。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Encore() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<EncorePower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
