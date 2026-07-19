using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 不動明王 — 敵が所有者へデバフを付与しようとするたび、付与元に Amount ダメージ。
/// アーティファクトで防がれても反射する（付与「試行」時点で発動）。
/// デバフ1回の付与につき1回（複数デバフならその回数だけ）。
/// </summary>
public class FudoMyooPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // AfterPowerAmountChanged はアーティファクトで量が0になると呼ばれない。
    // Before なら付与試行の元の量で必ず走る。
    public override async Task BeforePowerAmountChanged(
        PowerModel power,
        decimal amount,
        Creature target,
        Creature? applier,
        CardModel? cardSource)
    {
        if (Owner is not { IsAlive: true }) return;
        if (amount <= 0M) return;
        if (target != Owner) return;
        if (power.Type != PowerType.Debuff) return;
        // 自分自身へのスタック増減（例: このパワー）は対象外
        if (ReferenceEquals(power, this)) return;
        if (applier is not { IsAlive: true } || !applier.IsEnemy) return;

        Flash();
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            applier,
            Amount,
            ValueProp.Unpowered | ValueProp.SkipHurtAnim,
            Owner);
    }
}
