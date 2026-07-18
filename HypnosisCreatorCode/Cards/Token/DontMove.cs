using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>Don't Move! — 対象をスタンさせる。No.56系 調教命令。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class DontMove() : TrainingCommand
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await CreatureCmd.Stun(play.Target);
    }
}
