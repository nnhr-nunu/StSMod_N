using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>Come! — 引き寄せ＋DomSub性癖に目覚めさせる調教命令。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Come() : TrainingCommand
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub];
    public override bool PreferLeftWhenGenerated => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PullTracker.TryPull(play.Target, Owner.Creature);
        FetishCombat.Awaken(play.Target, FetishType.DomSub, Owner);
    }
}
