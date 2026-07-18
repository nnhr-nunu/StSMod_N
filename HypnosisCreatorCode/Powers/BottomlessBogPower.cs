using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>底なしの沼 — ターン終了時、各敵の（トランス＋沼）× Amount で破滅を付与する。</summary>
public class BottomlessBogPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || CombatState == null) return;
        if (side != CombatSide.Player) return;

        foreach (var enemy in CombatState.HittableEnemies.Where(e => e.IsAlive).ToList())
        {
            var stacks = TranceCombat.GetTrance(enemy) + enemy.GetPowerAmount<BogPower>();
            if (stacks <= 0) continue;
            await FetishCombat.ApplyDoom(choiceContext, enemy, stacks * Amount, Owner, null);
        }
    }
}
