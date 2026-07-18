using HypnosisCreator.HypnosisCreatorCode.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>性倒錯への理解 — 性癖タグ付きカードをプレイするたび、ブロックを得る。</summary>
public class FetishUnderstandingPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (cardPlay.Card is not HypnosisCreatorCard hc || hc.CardFetishes.Count == 0) return;

        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);
    }
}
