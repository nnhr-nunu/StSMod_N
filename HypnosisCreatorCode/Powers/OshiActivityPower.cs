using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 布教欲求 — カードプレイ時に指定した対象の破滅・沼を、自ターン終了時に他の敵全員へコピーする。
/// </summary>
public class OshiActivityPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public Creature? SourceEnemy { get; set; }
    public decimal GoldToGain { get; set; }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (Owner == null || !Owner.IsAlive) return;
        if (CombatState == null || SourceEnemy is not { IsAlive: true }) return;

        var doom = SourceEnemy.GetPowerAmount<DoomPower>();
        var bog = SourceEnemy.GetPowerAmount<BogPower>();
        if (doom <= 0 && bog <= 0) return;

        foreach (var enemy in CombatState.HittableEnemies.Where(e => e != SourceEnemy).ToList())
        {
            if (doom > 0) await PowerCmd.Apply<DoomPower>(choiceContext, enemy, doom, Owner, null);
            if (bog > 0) await PowerCmd.Apply<BogPower>(choiceContext, enemy, bog, Owner, null);
        }
    }
}
