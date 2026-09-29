using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>Cum! — 対象は体力19を失う調教命令。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Cum() : TrainingCommand
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("LoseHp", 19M)];

    protected override async Task OnCommandPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        // 「体力を失う」効果: ブロックで防げない（本modの他の HP 喪失カードと同じ ValueProp）
        await CreatureCmd.Damage(
            choiceContext, play.Target, DynamicVars["LoseHp"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, Owner.Creature, this, play);
    }
}
