using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Starter;

/// <summary>
/// 性癖の沼 — 敵の性癖スロットを常に可視化するスターター。
/// 敵出現時にランダム性癖を1つ付与する。
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
        // EnsureSpawnDefaults 内で HUD 更新まで行う
        EnemyFetishSlots.EnsureSpawnDefaults(creature, Owner, rng);
        await Task.CompletedTask;
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        FetishOrbHud.ClearAll();
        await Task.CompletedTask;
    }
}
