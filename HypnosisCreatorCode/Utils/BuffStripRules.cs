using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 敵のバフ解除（赤ちゃん催眠・Present! 等）で外してよいかを判定する。
/// 戦闘開始時から付く敵固有性能のうち、剥がすと撃破／復活フローが壊れるものは除外する。
/// </summary>
public static class BuffStripRules
{
    public static bool CanStripFromEnemy(PowerModel power)
    {
        if (power.Type != PowerType.Buff) return false;
        if (power.Owner is not { IsEnemy: true }) return false;

        // 性癖スロット表示用。剥がすと HUD が消えるがスロットデータは残るため再同期が必要になる。
        if (power is FetishAttributePower)
            return false;

        // 本家フック（幻影蟲 Illusion 等が false）
        if (!Hook.ShouldPowerBeRemovedOnDeath(power))
            return false;

        // 本家フック未登録だが、戦闘開始時付与の敵固有性能
        if (IsProtectedInnateEnemyBuff(power))
            return false;

        return true;
    }

    private static bool IsProtectedInnateEnemyBuff(PowerModel power) =>
        power is AdaptablePower
            or DieForYouPower
            or IllusionPower
            or ReattachPower
            or SkittishPower
            or SteamEruptionPower
            or SurprisePower;
}
