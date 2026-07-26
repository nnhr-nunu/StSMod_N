using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// このターン限りの飛翔。本家 SoarPower と同じく被攻撃ダメージを50%軽減。
/// 残り敵ターン数は Apply 時の Amount（心臓は2）。
/// </summary>
public class ThisTurnSoarPower : HypnosisCreatorPower
{
    private int _remainingEnemyTurns;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => "owl_magistrate_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "owl_magistrate_heart.png".BigRelicImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("DamageDecrease", 50m)];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        var turns = Math.Max(1, (int)Amount);
        _remainingEnemyTurns = Math.Max(_remainingEnemyTurns, turns);
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        return DynamicVars["DamageDecrease"].BaseValue / 100m;
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
