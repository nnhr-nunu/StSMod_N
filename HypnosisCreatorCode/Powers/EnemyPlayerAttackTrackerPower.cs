using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>敵のプレイヤーへの攻撃回数を <see cref="EnemyPlayerAttackTracker"/> へ記録する（非表示）。</summary>
public class EnemyPlayerAttackTrackerPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>バフ行に出さず、付与時の CustomScaledWait も避ける。</summary>
    protected override bool IsVisibleInternal => false;

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner) return Task.CompletedTask;
        if (dealer is not { IsEnemy: true }) return Task.CompletedTask;
        if (!props.IsPoweredAttack()) return Task.CompletedTask;
        EnemyPlayerAttackTracker.Record(dealer);
        return Task.CompletedTask;
    }
}
