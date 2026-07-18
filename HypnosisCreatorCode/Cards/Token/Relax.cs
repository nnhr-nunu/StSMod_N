using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>リラックスして — 対象の警戒を緩め脆弱を与える調教命令。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Relax() : TrainingCommand
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<FrailPower>(1M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<FrailPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<FrailPower>(
            choiceContext, play.Target, DynamicVars["FrailPower"].BaseValue, Owner.Creature, this);
    }
}
