using MegaCrit.Sts2.Core.Entities.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 敵の性癖属性。戦闘開始時からバフ行に表示し、ホバーで刺さり時の破滅量を示す。
/// 判定本体は <see cref="Utils.FetishCombat"/> / スロット側。
/// </summary>
public abstract class FetishAttributePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}
