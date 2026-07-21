using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ぬぬ地獄 — 引き寄せ効果のあるカードが追加で Amount ダメージを与える。
/// アイコンは貪りの獣の心臓レリックと同じ。
/// </summary>
public class NunuHellPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => "maw_beast_heart.png".RelicImagePath();
    public override string CustomBigIconPath => "maw_beast_heart.png".BigRelicImagePath();
}
