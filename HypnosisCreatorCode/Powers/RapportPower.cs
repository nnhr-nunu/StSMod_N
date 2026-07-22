using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ラポール — 自ターンの手札ドロー前、条件を満たせば手札カウントを Amount 進める。
/// UG済みを一度でもプレイしていれば無条件。未UGのみなら前ターン未攻撃時のみ。
/// </summary>
public class RapportPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>UG版ラポールを一度でも適用したら true（未UGとの混在でも優先）。</summary>
    public bool AlwaysAdvance { get; private set; }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (cardSource is { IsUpgraded: true })
            AlwaysAdvance = true;
        return Task.CompletedTask;
    }

    public override Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (Owner == null || player.Creature != Owner) return Task.CompletedTask;

        var turn = player.PlayerCombatState?.TurnNumber ?? 0;
        var attackedLastTurn = PlayerAttackTracker.AttackedOnTurn(player, turn - 1);
        if (!AlwaysAdvance && attackedLastTurn) return Task.CompletedTask;

        CountRules.AdvanceHandCountCards(player, Math.Max(1, Amount));
        return Task.CompletedTask;
    }
}
