using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>ASMR催眠 — 敵がプレイヤーから攻撃を受けた側を記録する（非表示）。</summary>
public class AsmrEnemySideTrackerPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || dealer is not { IsPlayer: true }) return;
        if (!props.IsPoweredAttack()) return;

        var player = dealer.Player;
        if (player == null) return;

        var side = AsmrSideRules.TryGetPlayerSide(player);
        if (side == null) return;

        await AsmrSideRules.SyncEnemyHitSideAsync(choiceContext, target, side.Value, dealer);
    }
}
