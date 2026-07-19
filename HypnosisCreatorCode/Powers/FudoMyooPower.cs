using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 不動明王 — 敵が所有者へデバフを付与するたび、付与元に Amount ダメージ。
/// デバフ1回の付与につき1回（複数デバフならその回数だけ）。
/// </summary>
public class FudoMyooPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (Owner is not { IsAlive: true }) return;
        if (amount <= 0M) return;
        if (power.Owner != Owner) return;
        if (power.Type != PowerType.Debuff) return;
        if (applier is not { IsAlive: true } || !applier.IsEnemy) return;

        Flash();
        await CreatureCmd.Damage(
            choiceContext,
            applier,
            Amount,
            ValueProp.Unpowered | ValueProp.SkipHurtAnim,
            Owner);
    }
}
