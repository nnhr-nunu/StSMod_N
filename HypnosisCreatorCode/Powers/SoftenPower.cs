using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ふにゃへにゃ — トランス1につき与ダメ減少。1枚目30%、重ねがけごとにさらに+10%。
/// </summary>
public class SoftenPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>トランス1スタックあたりの減少率（0.30, 0.40, …）。</summary>
    public decimal ReductionPerTrance => 0.20M + 0.10M * Math.Max(1, Amount);

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner) return 1M;
        if (dealer == null || !dealer.IsEnemy) return 1M;

        var trance = TranceCombat.GetTrance(dealer);
        if (trance <= 0) return 1M;

        return Math.Max(0.1M, 1M - ReductionPerTrance * trance);
    }
}
