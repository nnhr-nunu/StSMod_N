using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ふにゃへにゃ — トランス1スタックにつき与ダメ20%減。最大40%減（UGカード由来は60%）。
/// </summary>
public class SoftenPower : HypnosisCreatorPower
{
    public const decimal DefaultMaxReduction = 0.40M;
    public const decimal UpgradedMaxReduction = 0.60M;

    private const decimal ReductionPerTrance = 0.20M;

    public decimal MaxReductionCap { get; set; } = DefaultMaxReduction;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner) return 1M;
        if (dealer == null || !dealer.IsEnemy) return 1M;

        var softenPowers = Owner?.Powers.OfType<SoftenPower>().ToList();
        if (softenPowers is not { Count: > 0 }) return 1M;
        if (softenPowers[0] != this) return 1M;

        var trance = TranceCombat.GetTrance(dealer);
        if (trance <= 0) return 1M;

        var maxReduction = softenPowers.Max(p => p.MaxReductionCap);
        var reduction = Math.Min(maxReduction, ReductionPerTrance * trance);
        return 1M - reduction;
    }
}
