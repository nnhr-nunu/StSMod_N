using HypnosisCreator.HypnosisCreatorCode.Utils;
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
        // 集団催眠の波及 AutoPlay は「1枚の論理プレイ」の続き。プレイヤー副作用は初回のみ。
        if (MassHypnosisPower.IsPropagating) return;
        if (!CardFetishLookup.HasAnyFetish(cardPlay.Card)) return;

        // Feel No Pain と同様 Unpowered: Amount＝カード表記。Move だと敏捷が乗って説明より多くなる。
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
