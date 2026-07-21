using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 状態異常催眠 — トランス敵へ状態異常・呪いをプレイ可能にし、
/// 手元／これから得る対応状態異常をプレイ可能版へ置き換える。
/// </summary>
public class StatusHypnosisPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var player = Owner?.Player;
        if (player == null) return;
        await StatusHypnosisConvert.ConvertAllCombatStatuses(player);
    }

    public override async Task AfterCardChangedPiles(
        CardModel card, PileType oldPileType, AbstractModel? source)
    {
        var player = Owner?.Player;
        if (player == null || card.Owner != player) return;
        await StatusHypnosisConvert.TryConvertAsync(card, player);
    }

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        var player = Owner?.Player;
        if (player == null || card.Owner != player) return;
        await StatusHypnosisConvert.TryConvertAsync(card, player);
    }

    public override async Task AfterCardGeneratedForCombat(CardModel card, MegaCrit.Sts2.Core.Entities.Players.Player? player)
    {
        if (player == null || Owner?.Player != player) return;
        await StatusHypnosisConvert.TryConvertAsync(card, player);
    }
}
