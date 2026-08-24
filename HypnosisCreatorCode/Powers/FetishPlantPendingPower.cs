using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 初心者向け催眠 — 催眠対象へ性癖カードを使ったとき、未所持の性癖を植え付ける予約。
/// 実体は <see cref="FetishPlantPending"/> が保持する。植え付けが実際に起きた対象だけ消費する。
/// </summary>
public class FetishPlantPendingPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var player = Owner?.Player;
        if (player == null) return;
        if (cardPlay.Card.Owner != player) return;
        if (!CardFetishLookup.HasAnyFetish(cardPlay.Card)) return;

        var fetishes = CardFetishLookup.GetFetishes(cardPlay.Card);
        await FetishPlantPending.TryConsumeOnPlay(
            choiceContext, player, cardPlay, fetishes, cardPlay.Card);
    }
}
