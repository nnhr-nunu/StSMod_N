using MegaCrit.Sts2.Core.Entities.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 状態異常催眠 — CSV: トランス中の敵へ状態異常・呪いを付与しやすくする効果を想定。
/// 個別の状態異常・呪い付与効果は mechanics-lock.md の「後送」項目のため未確定。
/// 暫定でマーカーパワーのみ実装し、対象カードが実装された際にこのパワーの有無を判定条件に使う。
/// TODO: 状態異常・呪い個別カードの仕様が確定したら効果を実装する。
/// </summary>
public class StatusHypnosisPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}
