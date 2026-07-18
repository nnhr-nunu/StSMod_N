using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 練達 — パワー。有効な間にプレイしたカウントカードを、戦闘終了時に永続アップグレードする。
/// CSV: 支援カードのためカウントキーワードは付けない（mechanics-lock.md）。UGでコスト1。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Mastery() : HypnosisCreatorCard(2,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<MasteryPower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
