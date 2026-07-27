using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 敵のデバフ解除（メンタルケア）で外してよいかを判定する。
/// </summary>
public static class DebuffStripRules
{
    public static bool CanStripFromEnemyForMentalCare(PowerModel power)
    {
        if (power.Type != PowerType.Debuff) return false;
        if (power is DoomPower) return false;
        if (power.Owner is not { IsEnemy: true }) return false;

        // 好き好き催眠は残す（睡眠見た目・スライム／心停止／カタレプシー／トランス／沼は解除可）
        if (power is LoveHypnosisPower)
            return false;

        return true;
    }
}
