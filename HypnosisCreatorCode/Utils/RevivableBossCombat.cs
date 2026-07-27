using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 本家 <see cref="AdaptablePower"/> 付きの多段ボス（実験体など）。
/// 破滅・心停止など即死経路でも形態移行できるよう、戦闘除去と終了判定を守る。
/// </summary>
public static class RevivableBossCombat
{
    /// <summary>
    /// まだ最終形態ではなく、AdaptablePower による復活／形態移行が残っているか。
    /// <see cref="MonsterModel.ShouldDisappearFromDoom"/> が false のときは最終とどめ（実験体は Respawns==0）。
    /// 生存中の誤付与 AdaptablePower では止めない（死亡後の形態移行待ちのみ）。
    /// </summary>
    public static bool HasPendingRevival(Creature? creature)
    {
        if (creature is not { IsEnemy: true, Monster: { } monster }) return false;
        if (creature.GetPower<AdaptablePower>() is null) return false;
        if (!monster.ShouldDisappearFromDoom) return false;
        // 形態移行待ちは破滅等で一度死亡したあと。生存中の誤付与では戦闘終了を止めない。
        if (creature.IsAlive) return false;
        return true;
    }

    public static bool CombatHasPendingRevival(ICombatState? combatState)
    {
        if (combatState == null) return false;

        foreach (CombatSide side in Enum.GetValues<CombatSide>())
        {
            foreach (var creature in combatState.GetCreaturesOnSide(side))
            {
                if (HasPendingRevival(creature)) return true;
            }
        }

        return false;
    }
}
