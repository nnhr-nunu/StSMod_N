using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 外骨格蟲の心臓 — 本家 <see cref="HardToKillPower"/> と同じ被ダメ／HP喪失の上限（Amount）。
/// 心臓発動時は残り敵ターン2。解除は敵ターン終了ごとに1消費。
/// </summary>
public class ExoskeletonBugBuffPower : HypnosisCreatorPower
{
    private static readonly PowerModel VanillaHardToKill = ModelDb.Power<HardToKillPower>();

    public const int DefaultHeartTurns = 2;

    private int _remainingEnemyTurns;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => VanillaHardToKill.PackedIconPath;
    public override string CustomBigIconPath => VanillaHardToKill.ResolvedBigIconPath;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (_remainingEnemyTurns <= 0)
            _remainingEnemyTurns = DefaultHeartTurns;
        else
            _remainingEnemyTurns = TurnScopedDuration.AddStack(_remainingEnemyTurns);
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageCap(
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner) return decimal.MaxValue;
        return Amount;
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy) return;
        if (Owner == null || !Owner.IsAlive) return;
        if (!participants.Contains(Owner)) return;
        if (!TurnScopedDuration.Consume(ref _remainingEnemyTurns))
            return;
        await PowerCmd.Remove(this);
    }
}
