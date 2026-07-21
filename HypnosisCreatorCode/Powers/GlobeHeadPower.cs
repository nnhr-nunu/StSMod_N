using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>グローブヘッド — パワーカードプレイ時にブロック（Amount）を得る。</summary>
public class GlobeHeadPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => "globe_head_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "globe_head_heart.png".BigRelicImagePath();

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (cardPlay.Card.Type != CardType.Power) return;
        if (Amount <= 0) return;

        // パワーカード経由なので CardPlay を渡し、敏捷が自動で乗る（ValueProp.Move）
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, cardPlay);
    }
}
