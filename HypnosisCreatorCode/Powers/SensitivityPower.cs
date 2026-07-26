using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// このターン、所有者への攻撃ダメージを3倍にする（カード名の3000倍はフレーバー）。
/// Amount＝残り自分ターン数。重ねがけで+1ターン延長。
/// </summary>
public class SensitivityPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

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
        return 3m;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner)) return;

        if (Amount <= 1)
            await PowerCmd.Remove(this);
        else
            await PowerCmd.Decrement(this);
    }
}
