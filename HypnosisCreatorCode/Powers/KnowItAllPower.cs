using MegaCrit.Sts2.Core.Entities.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ぜんぶ知ってるよ — Amount＝性癖刺さり効果の総倍率（初回2、重ねがけごとに+1）。
/// 破滅量は <see cref="Utils.FetishCombat.ResolveFetishHitMultiplier"/> 経由。
/// </summary>
public class KnowItAllPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
