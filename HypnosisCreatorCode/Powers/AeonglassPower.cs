using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 永劫の砂時計 — カードを6枚プレイするたび、プレイ可能な衰微を手札に1枚加える。
/// </summary>
public class AeonglassPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => "aeonglass_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "aeonglass_heart.png".BigRelicImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar(WitherOmen.CardsLeftKey, WitherOmen.DefaultCardsLeft)];

    public override int DisplayAmount => DynamicVars[WitherOmen.CardsLeftKey].IntValue;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (MassHypnosisPower.IsPropagating) return;

        // 衰微プレイ自体はカウントせず、スタックを6に固定（備考の使用後リセット）
        if (cardPlay.Card is AbnormalWither)
        {
            WitherOmen.ResetPower(this);
            InvokeDisplayAmountChanged();
            return;
        }

        var left = DynamicVars[WitherOmen.CardsLeftKey];
        left.BaseValue = left.BaseValue - 1m;
        InvokeDisplayAmountChanged();

        if (left.IntValue > 0) return;

        await Cmd.Wait(0.5f);
        await StatusHypnosisConvert.AddFreePlayableAsync<AbnormalWither>(
            Owner.Player!, 1, PileType.Hand);
        Flash();
        WitherOmen.ResetPower(this);
        InvokeDisplayAmountChanged();
    }
}
