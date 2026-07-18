using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>受容の需要 — 受けた攻撃の回数を記録し、次の自ターン開始時にその数だけエナジーとドローを得る。</summary>
public class AcceptanceNeedPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private int _hitsTaken;

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return Task.CompletedTask;
        if (!props.IsPoweredAttack()) return Task.CompletedTask;
        _hitsTaken++;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player.Creature != Owner) return;
        if (_hitsTaken <= 0) return;

        var amount = _hitsTaken;
        _hitsTaken = 0;
        await PlayerCmd.GainEnergy(amount, player);
        await CardPileCmd.Draw(choiceContext, amount, player);
    }
}
