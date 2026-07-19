using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ラポール — 自ターンの手札ドロー前、条件を満たせば手札カウントを追加で1進める。
/// Amount &gt;= 2（UG）なら無条件。それ以外は前ターン未攻撃時のみ。
/// ドロー後だと今引いたカードにもかかるため <see cref="BeforeHandDraw"/> で処理する。
/// </summary>
public class RapportPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (Owner == null || player.Creature != Owner) return Task.CompletedTask;

        var turn = player.PlayerCombatState?.TurnNumber ?? 0;
        var alwaysAdvance = Amount >= 2;
        var attackedLastTurn = PlayerAttackTracker.AttackedOnTurn(player, turn - 1);
        if (!alwaysAdvance && attackedLastTurn) return Task.CompletedTask;

        CountRules.AdvanceHandCountCards(player);
        return Task.CompletedTask;
    }
}
