using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 初心者向け催眠 — 次にプレイする性癖カードのタグを、催眠対象の敵へ植え付ける予約。
/// 実体は <see cref="FetishPlantPending"/> が保持する。
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
        // 集団催眠の AutoPlay 波及では消費しない（手動プレイ1回でアーム済み全員へ植え付け済み）
        if (MassHypnosisPower.IsPropagating) return;
        if (!CardFetishLookup.HasAnyFetish(cardPlay.Card)) return;

        var fetishes = CardFetishLookup.GetFetishes(cardPlay.Card);
        await FetishPlantPending.TryConsumeOnPlay(
            choiceContext, player, cardPlay.Target, fetishes, cardPlay.Card);
    }
}
