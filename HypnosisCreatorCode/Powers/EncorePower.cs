using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// アンコール — プレイしたカウントカードが廃棄される代わりに手札へ戻る。
/// CSV: 「ベースコストに戻す」は本カードのコスト計算簡略化のため未実装（既存の一時的コスト変動はそのまま）。
/// TODO: コストを完全にベース値へ戻す処理が必要になったら追加する。
/// </summary>
public class EncorePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;
        var card = cardPlay.Card;
        if (card.Owner?.Creature != Owner) return;
        if (!CountRules.HasCountKeyword(card)) return;
        if (card.Pile?.Type != PileType.Exhaust) return;

        await CardPileCmd.Add(card, PileType.Hand);
    }
}
