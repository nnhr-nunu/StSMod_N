using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>トランスして — 対象にトランスを直接付与する調教命令。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Trance() : TrainingCommand
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Trance", 1M)];

    protected override async Task OnCommandPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
    }
}
