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
/// ご主人様の言うとおり — 性癖カードをプレイするたび、タグ種別で自己強化する。
/// UG: Abn 筋力1+敏捷1 / SM 活力3 / DomSub ブロック2。
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

        var upgraded = Amount >= 2;
        foreach (var fetish in hc.CardFetishes.Distinct())
        {
            switch (fetish)
            {
                case FetishType.Abnormal:
                    await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 1M, Owner, cardPlay.Card);
                    if (upgraded)
                        await PowerCmd.Apply<DexterityPower>(choiceContext, Owner, 1M, Owner, cardPlay.Card);
                    break;
                case FetishType.Sm:
                    await PowerCmd.Apply<VigorPower>(
                        choiceContext, Owner, upgraded ? 3M : 2M, Owner, cardPlay.Card);
                    break;
                case FetishType.DomSub:
                    await CreatureCmd.GainBlock(Owner, upgraded ? 2M : 1M, ValueProp.Move, null);
                    break;
            }
        }
    }
}
