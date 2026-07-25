using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// ミラーリングのダメージ解決ルール。意図値ミラー／旧補正付きの切替はここを正とする。
/// </summary>
public static class MirroringRules
{
    /// <summary>敵意図どおり（連撃・1ヒット値そのまま、追加の状態異常補正なし）。</summary>
    public static bool UseIntentValuesOnly => MirroringDamagePreview.UseIntentValuesOnly;

    /// <summary>実プレイ時の ValueProp。意図値ミラー時は Unpowered（筋力・スロウ等を載せない）。</summary>
    public static ValueProp DamageProps =>
        UseIntentValuesOnly ? ValueProp.Unpowered : ValueProp.Move;

    /// <summary>意図値ミラー時はプレイヤー側の脱力・筋力等を載せない。</summary>
    public static AttackCommand ApplyDamageProps(AttackCommand cmd) =>
        UseIntentValuesOnly ? cmd.Unpowered() : cmd;
}
