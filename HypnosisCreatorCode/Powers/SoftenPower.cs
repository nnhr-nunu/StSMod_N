using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ふにゃへにゃ — トランス1スタックにつき与ダメ20%減。合計最大50%減。
/// </summary>
public class SoftenPower : HypnosisCreatorPower
{
    private const decimal ReductionPerTrance = 0.20M;
    private const decimal MaxReduction = 0.50M;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner) return 1M;
        if (dealer == null || !dealer.IsEnemy) return 1M;

        var primary = Owner?.Powers.OfType<SoftenPower>().FirstOrDefault();
        if (primary != this) return 1M;

        var trance = TranceCombat.GetTrance(dealer);
        if (trance <= 0) return 1M;

        var reduction = Math.Min(MaxReduction, ReductionPerTrance * trance);
        return 1M - reduction;
    }
}
