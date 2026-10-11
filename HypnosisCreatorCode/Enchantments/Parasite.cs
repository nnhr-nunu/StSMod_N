using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Enchantments;

/// <summary>
/// 寄生 — 感染で付けるエンチャント。効果は本家 Sharp と同じ（アタックのダメージ＋Amount）。
/// アイコンは本家 Sharp のものを流用する。
/// </summary>
public sealed class Parasite : CustomEnchantmentModel
{
    public override bool ShowAmount => true;

    protected override string? CustomIconPath => ImageHelper.GetImagePath("enchantments/sharp.png");

    public override bool CanEnchantCardType(CardType cardType) => cardType == CardType.Attack;

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props) =>
        props.IsPoweredAttack() ? Amount : 0m;
}
