using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 実験体など AdaptablePower 多段ボスが、破滅・心停止・即死 Kill で戦闘終了しないよう守る。
/// 本家は ShouldCreatureBeRemovedFromCombatAfterDeath=false で敵リストに残すが、
/// 即死経路の RemoveCreature がステートマシンをリセットし、復活待ちでも戦闘が終わることがある。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ShouldCreatureBeRemovedFromCombatAfterDeath))]
public static class RevivableBossKeepInCombatPatch
{
    public static void Postfix(Creature creature, ref bool __result)
    {
        if (!__result) return;
        if (RevivableBossCombat.HasPendingRevival(creature))
            __result = false;
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.ShouldStopCombatFromEnding))]
public static class RevivableBossStopCombatEndPatch
{
    public static void Postfix(ICombatState combatState, ref bool __result)
    {
        if (__result) return;
        if (RevivableBossCombat.CombatHasPendingRevival(combatState))
            __result = true;
    }
}

[HarmonyPatch(typeof(CombatState), nameof(CombatState.RemoveCreature))]
public static class RevivableBossCombatStateRemovePatch
{
    public static bool Prefix(Creature creature) =>
        !RevivableBossCombat.HasPendingRevival(creature);
}

[HarmonyPatch(typeof(CombatManager), nameof(CombatManager.RemoveCreature))]
public static class RevivableBossCombatManagerRemovePatch
{
    public static bool Prefix(Creature creature) =>
        !RevivableBossCombat.HasPendingRevival(creature);
}
