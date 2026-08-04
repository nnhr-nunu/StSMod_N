using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ふにゃへにゃ — トランス1スタックあたりの減少率 = 10%＋10%×本パワースタック（1枚20%、2枚30%…）。
/// 合計は最大40%（UGカード由来は60%）。下限ダメージ倍率10%。
/// </summary>
public class SoftenPower : HypnosisCreatorPower
{
    public const decimal DefaultMaxReduction = 0.40M;
    public const decimal UpgradedMaxReduction = 0.60M;

    public decimal MaxReductionCap { get; set; } = DefaultMaxReduction;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    /// <summary>トランス1スタックあたりの減少率（本パワーのスタック数で上がる）。</summary>
    public decimal ReductionPerTranceStack =>
        0.10M + 0.10M * Math.Max(1, Amount);

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props, Creature? dealer,
        CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner) return 1M;
        if (dealer == null || !dealer.IsEnemy) return 1M;

        var soften = Owner?.GetPower<SoftenPower>();
        if (soften == null || soften != this) return 1M;

        var trance = TranceCombat.GetTrance(dealer);
        if (trance <= 0) return 1M;

        var reduction = Math.Min(MaxReductionCap, ReductionPerTranceStack * trance);
        return Math.Max(0.10M, 1M - reduction);
    }
}
