using HypnosisCreator.HypnosisCreatorCode.Extensions;
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
/// プレイヤーはこのターン限り（本家敵は戦闘中ずっと）。解除は敵ターン終了（本家 Intangible 同様）。
/// </summary>
public class ExoskeletonBugBuffPower : HypnosisCreatorPower
{
    private static readonly PowerModel VanillaHardToKill = ModelDb.Power<HardToKillPower>();

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => VanillaHardToKill.PackedIconPath;
    public override string CustomBigIconPath => VanillaHardToKill.ResolvedBigIconPath;

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
        // プレイヤーターン終了では外さない（敵の攻撃前に消えるため）。本家 Intangible と同じ。
        if (side != CombatSide.Enemy) return;
        if (Owner == null || !Owner.IsAlive) return;
        if (!participants.Contains(Owner)) return;
        await PowerCmd.Remove(this);
    }
}
