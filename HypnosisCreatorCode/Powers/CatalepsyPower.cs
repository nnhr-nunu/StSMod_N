using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// カタレプシー — 本家スロウ相当（自ターン開始時にリセット）を対象の敵に付与する独自実装。
/// 本家 SlowPower は所有者自身がカードをプレイした回数で増える設計のため敵には機能しない。
/// アップグレード後は対象がトランス中ならリセットをスキップする（<see cref="PersistIfTranced"/>）。
/// </summary>
public class CatalepsyPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public bool PersistIfTranced { get; set; }

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner) return 1M;
        if (!props.IsPoweredAttack()) return 1M;
        return 1M + 0.1M * Amount;
    }

    public override Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Owner == null || !participants.Contains(Owner)) return Task.CompletedTask;
        if (PersistIfTranced && TranceCombat.HasTrance(Owner)) return Task.CompletedTask;
        return PowerCmd.Remove(this);
    }
}
