using MegaCrit.Sts2.Core.Entities.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 状態異常催眠 — トランス状態の敵へ、敵対象の状態異常・呪いをプレイできるようにする。
/// 粘液（AbnormalSlime）など個別カードの効果は各カード側。プレイ可否は StatusHypnosisRules。
/// </summary>
public class StatusHypnosisPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}
