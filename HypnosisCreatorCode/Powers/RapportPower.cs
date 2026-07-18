using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ラポール — 自ターン開始時、手札のカウントカードのコストをベースの1減少に加えて追加で-1する。
/// 前ターンに敵を攻撃していなければさらに-1（Amount &gt;= 2 のときは常に、UGで無条件化）。
/// </summary>
public class RapportPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (Owner == null || player.Creature != Owner) return Task.CompletedTask;

        var hand = player.PlayerCombatState?.Hand;
        if (hand == null) return Task.CompletedTask;

        var turn = player.PlayerCombatState?.TurnNumber ?? 0;
        var alwaysExtra = Amount >= 2;
        var attackedLastTurn = PlayerAttackTracker.AttackedOnTurn(player, turn - 1);
        var extra = 1 + ((alwaysExtra || !attackedLastTurn) ? 1 : 0);

        foreach (var card in hand.Cards.ToList())
        {
            if (!card.Keywords.Contains(HcKeywords.Count)) continue;
            if (card.EnergyCost.GetWithModifiers(MegaCrit.Sts2.Core.Entities.Cards.CostModifiers.All) <= 0) continue;
            card.EnergyCost.AddThisTurn(-extra);
        }

        return Task.CompletedTask;
    }
}
