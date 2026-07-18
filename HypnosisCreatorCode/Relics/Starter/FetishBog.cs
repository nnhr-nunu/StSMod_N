using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Starter;

/// <summary>
/// 性癖の沼 — 敵の性癖スロットを常に可視化するスターター。
/// 敵出現時に SM/DomSub/アブノーマルのいずれか1つを付与する。
/// </summary>
public class FetishBog : HypnosisCreatorRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (Owner == null) return;
        if (!creature.IsEnemy) return;

        Flash();
        var rng = Owner.RunState.Rng.CombatOrbGeneration;
        EnemyFetishSlots.EnsureSpawnDefaults(creature, Owner, rng);
        await Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || Owner == null) return;
        var combat = Owner.Creature.CombatState;
        if (combat == null) return;

        var rng = Owner.RunState.Rng.CombatOrbGeneration;
        foreach (var enemy in combat.HittableEnemies.ToList())
            EnemyFetishSlots.EnsureSpawnDefaults(enemy, Owner, rng);

        await Task.CompletedTask;
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        FetishOrbHud.ClearAll();
        await Task.CompletedTask;
    }
}
