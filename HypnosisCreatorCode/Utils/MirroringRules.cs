using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// ミラーリングのダメージ解決ルール。意図値ミラー／旧補正付きの切替はここを正とする。
/// </summary>
public static class MirroringRules
{
    /// <summary>敵意図どおり（連撃・1ヒット値そのまま、追加の状態異常補正なし）。</summary>
    public static bool UseIntentValuesOnly = true; // 常に Unpowered 計算、筋力・脱力は除外
    
    public static ValueProp DamageProps => ValueProp.Unpowered; // 常に Unpowered
    
    public static AttackCommand ApplyDamageProps(AttackCommand cmd) => cmd.Unpowered();
}
