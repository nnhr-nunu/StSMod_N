using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Relics.Starter;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using HcCharacter = HypnosisCreator.HypnosisCreatorCode.Character.HypnosisCreator;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

internal static class FetishOwnerLookup
{
    public static Player? Find(ICombatState combatState)
    {
        foreach (var player in combatState.Players)
        {
            if (player.Character is HcCharacter) return player;
            if (player.Relics.Any(r => r is FetishBog)) return player;
            if (player.Character?.Id.Entry.Contains("HYPNOSISCREATOR", StringComparison.OrdinalIgnoreCase) == true)
                return player;
        }

        return null;
    }

    public static void EnsureAllEnemies(ICombatState combatState)
    {
        var owner = Find(combatState);
        if (owner == null) return;

        var rng = owner.RunState.Rng.CombatOrbGeneration;
        foreach (var enemy in combatState.HittableEnemies.ToList())
            EnemyFetishSlots.EnsureSpawnDefaults(enemy, owner, rng);
    }
}

/// <summary>敵出現時に性癖を付与し頭上HUDを出す。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCreatureAddedToCombat))]
public static class FetishHudCombatPatch
{
    public static void Postfix(ICombatState combatState, Creature creature)
    {
        if (!creature.IsEnemy) return;

        var owner = FetishOwnerLookup.Find(combatState);
        if (owner == null) return;

        var rng = owner.RunState.Rng.CombatOrbGeneration;
        EnemyFetishSlots.EnsureSpawnDefaults(creature, owner, rng);
    }
}

/// <summary>戦闘開始時に全敵を再初期化／再描画。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCombatStart))]
public static class FetishHudBeforeCombatPatch
{
    public static void Postfix(IRunState runState, ICombatState combatState)
    {
        FetishOwnerLookup.EnsureAllEnemies(combatState);

        var owner = FetishOwnerLookup.Find(combatState);
        if (owner == null) return;
        if (owner.Creature.GetPower<EnemyPlayerAttackTrackerPower>() != null) return;

        PowerCmd.Apply<EnemyPlayerAttackTrackerPower>(
                null!, owner.Creature, 1M, owner.Creature, null!)
            .GetAwaiter()
            .GetResult();
    }
}

/// <summary>
/// セーブ続き／取りこぼし対策。プレイヤーターン開始時にも全敵へ性癖＋HUDを保証する。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
public static class FetishHudTurnStartPatch
{
    public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, Player player)
    {
        if (FetishOwnerLookup.Find(combatState) == null) return;
        FetishOwnerLookup.EnsureAllEnemies(combatState);
    }
}
