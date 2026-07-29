using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Ancient;

/// <summary>
/// デトックス（No.108）— 古代パワー。戦闘終了時に任意でデッキ1枚を永続UG。
/// 薄汚れた本で入手時はレリック側で必ずUG済みになる。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Detox() : HypnosisCreatorCard(3,
    CardType.Power, CardRarity.Ancient,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play) =>
        await PowerCmd.Apply<DetoxPower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
