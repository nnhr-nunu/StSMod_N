namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 本家は <c>Hook.ModifyDamage</c> の戻りを小数のまま PreviewValue に入れ、表示時だけ <c>(int)</c>（0方向切り捨て）する。
/// 途中で切り捨てると、スロウ×先付与弱体のように倍率を重ねたとき実ダメージと1ずれる。
/// </summary>
public static class CombatPreviewMath
{
    public static decimal ScaleByUpcomingVulnerable(
        decimal damageAtCurrentVulnerable,
        decimal oldFactor,
        decimal newFactor)
    {
        if (oldFactor <= 0m)
            return Math.Max(0m, damageAtCurrentVulnerable);

        return Math.Max(0m, damageAtCurrentVulnerable * newFactor / oldFactor);
    }
}
