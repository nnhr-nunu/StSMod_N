using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 蠢く群生体の心臓 — 硬い殻と同趣旨。心臓の重ねがけで上限が半減（20→10→5…、最低1）。
/// </summary>
public class SwarmingHiveShellPower : HypnosisCreatorPower
{
    private const int BaseCap = 20;

    private class Data
    {
        public decimal damageReceivedThisTurn;
        public int heartApplications = 1;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool ShouldScaleInMultiplayer => true;

    public override string CustomPackedIconPath => "swarming_hive_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "swarming_hive_heart.png".BigRelicImagePath();

    public override int DisplayAmount =>
        (int)Math.Max(0m, (decimal)Amount - GetInternalData<Data>().damageReceivedThisTurn);

    protected override object InitInternalData() => new Data();

    public static int CapForApplications(int applications) =>
        Math.Max(1, BaseCap >> (Math.Max(1, applications) - 1));

    public async Task SyncApplications(PlayerChoiceContext ctx, Creature applier, int applications)
    {
        var data = GetInternalData<Data>();
        data.heartApplications = Math.Max(1, applications);
        await SyncCapAmount(ctx, applier);
    }

    public async Task StackFromHeartActivation(PlayerChoiceContext ctx, Creature applier)
    {
        var data = GetInternalData<Data>();
        data.heartApplications = Math.Max(1, data.heartApplications) + 1;
        await SyncCapAmount(ctx, applier);
        Flash();
    }

    private async Task SyncCapAmount(PlayerChoiceContext ctx, Creature applier)
    {
        var target = CapForApplications(GetInternalData<Data>().heartApplications);
        var delta = target - Amount;
        if (delta == 0) return;

        await PowerCmd.ModifyAmount(ctx, this, delta, applier, null);
        InvokeDisplayAmountChanged();
    }

    public override decimal ModifyHpLostBeforeOstyLate(
        Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || amount == 0m) return amount;

        return Math.Min(amount, (decimal)Amount - GetInternalData<Data>().damageReceivedThisTurn);
    }

    public override Task AfterModifyingHpLostBeforeOsty()
    {
        Flash();
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || result.WasFullyBlocked) return Task.CompletedTask;

        var data = GetInternalData<Data>();
        data.damageReceivedThisTurn += (decimal)result.UnblockedDamage;
        InvokeDisplayAmountChanged();
        if (data.damageReceivedThisTurn >= (decimal)Amount)
            Owner!.HpDisplay = HpDisplay.InfiniteWithNumbers;

        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        var data = GetInternalData<Data>();
        data.damageReceivedThisTurn = default;
        InvokeDisplayAmountChanged();
        if (Owner != null)
            Owner.HpDisplay = HpDisplay.Normal;

        return Task.CompletedTask;
    }
}
