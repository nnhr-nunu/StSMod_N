using HypnosisCreator.HypnosisCreatorCode.Extensions;
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
/// このターン限りの飛翔。本家 SoarPower と同じく被攻撃ダメージを50%軽減し、プレイヤーターン終了で消える。
/// </summary>
public class ThisTurnSoarPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => "owl_magistrate_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "owl_magistrate_heart.png".BigRelicImagePath();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("DamageDecrease", 50m)];

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        // 本家 SoarPower と同じ条件・倍率
        if (target != Owner) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        return DynamicVars["DamageDecrease"].BaseValue / 100m;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (Owner == null || !Owner.IsAlive) return;
        await PowerCmd.Remove(this);
    }
}
