using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// カタレプシー（UG）— 対象がトランス中なら、本家 <see cref="SlowPower"/> のターン開始リセットを打ち消す。
/// </summary>
public class CatalepsyPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    private decimal _savedSlowAmount;

    public override Task AfterSideTurnEnd(
        MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || side != CombatSide.Player) return Task.CompletedTask;
        var slow = Owner.GetPower<SlowPower>();
        if (slow != null)
            _savedSlowAmount = slow.DynamicVars["SlowAmount"].BaseValue;
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner == null || !participants.Contains(Owner)) return Task.CompletedTask;
        if (!TranceCombat.HasTrance(Owner)) return Task.CompletedTask;
        if (_savedSlowAmount <= 0M) return Task.CompletedTask;

        var slow = Owner.GetPower<SlowPower>();
        if (slow == null) return Task.CompletedTask;

        // SlowPower のリセット後に積み直す（フック順に依存しないよう、保存値で上書き）
        slow.DynamicVars["SlowAmount"].BaseValue = _savedSlowAmount;
        return Task.CompletedTask;
    }
}
