using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>ふにゃへにゃ — トランス1につき与ダメ20%減（最大40%、UGで最大60%）。コスト2。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Soften() : HypnosisCreatorCard(2,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var power = await PowerCmd.Apply<SoftenPower>(
            choiceContext, Owner.Creature, 1M, Owner.Creature, this);
        if (power != null && IsUpgraded)
            power.MaxReductionCap = SoftenPower.UpgradedMaxReduction;
    }
}
