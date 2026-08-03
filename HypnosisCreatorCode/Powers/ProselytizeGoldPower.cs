using HypnosisCreator.HypnosisCreatorCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 布教欲求の戦闘終了ゴールド累計。アイコン数字＝報酬画面に載る合計ゴールド。
/// </summary>
public class ProselytizeGoldPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override string CustomPackedIconPath => "proselytize.png".CardImagePath();
    public override string CustomBigIconPath => "proselytize.png".BigCardImagePath();
}
