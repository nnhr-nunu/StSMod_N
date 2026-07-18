using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 催眠クリエイター戦闘中、敵出現時に性癖スロット初期化と頭上オーブHUDを確実に出す。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCreatureAddedToCombat))]
public static class FetishHudCombatPatch
{
    public static void Postfix(ICombatState combatState, Creature creature)
    {
        if (!creature.IsEnemy) return;

        var owner = combatState.Players.FirstOrDefault(p => p.Character is HcCharacter);
        if (owner == null) return;

        var rng = owner.RunState.Rng.CombatOrbGeneration;
        EnemyFetishSlots.EnsureSpawnDefaults(creature, owner, rng);
    }
}
