using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 推し活 — 敵へのデバフ。プレイヤーターン終了時、自身の破滅・沼と同値を他の敵全員へコピーする。
/// </summary>
public class OshiActivityPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (Owner is not { IsAlive: true } || CombatState == null) return;

        var doom = Owner.GetPowerAmount<DoomPower>();
        var bog = Owner.GetPowerAmount<BogPower>();
        if (doom <= 0 && bog <= 0) return;

        foreach (var enemy in CombatState.HittableEnemies.Where(e => e != Owner && e.IsAlive).ToList())
        {
            if (doom > 0)
                await PowerCmd.Apply<DoomPower>(choiceContext, enemy, doom, Owner, null);
            if (bog > 0)
                await PowerCmd.Apply<BogPower>(choiceContext, enemy, bog, Owner, null);
        }
    }
}
