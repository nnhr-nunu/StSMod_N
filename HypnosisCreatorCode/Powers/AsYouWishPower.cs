using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ご主人様の言うとおり — 性癖タグ付きカードをプレイするたび、タグの種類に応じて自己強化する。
/// アブノーマル→筋力+1、SM→活力+2、DomSub→ブロック+1。アップグレード後は数値が上昇する。
/// </summary>
public class AsYouWishPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (cardPlay.Card is not HypnosisCreatorCard hc || hc.CardFetishes.Count == 0) return;

        var bonus = Amount >= 2 ? 2M : 1M;
        foreach (var fetish in hc.CardFetishes.Distinct())
        {
            switch (fetish)
            {
                case FetishType.Abnormal:
                    await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, bonus, Owner, cardPlay.Card);
                    break;
                case FetishType.Sm:
                    await PowerCmd.Apply<VigorPower>(choiceContext, Owner, bonus * 2, Owner, cardPlay.Card);
                    break;
                case FetishType.DomSub:
                    await CreatureCmd.GainBlock(Owner, bonus, ValueProp.Move, null);
                    break;
            }
        }
    }
}
