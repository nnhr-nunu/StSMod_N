using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>Don't Move! — 対象は筋力2を失う。No.63 調教命令。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class DontMove() : TrainingCommand
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<StrengthPower>("StrengthLoss", 2M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<StrengthPower>()];

    protected override async Task OnCommandPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, play.Target, -DynamicVars["StrengthLoss"].BaseValue, Owner.Creature, this);
    }
}
