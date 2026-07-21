using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>Look! — 破滅5を付与する調教命令。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Look() : TrainingCommand
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Doom", 5M)];

    protected override async Task OnCommandPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await FetishCombat.ApplyDoom(
            choiceContext, play.Target, DynamicVars["Doom"].IntValue, Owner.Creature, this);
    }
}
