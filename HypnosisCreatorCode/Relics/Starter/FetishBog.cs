using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Starter;

/// <summary>
/// 性癖の沼 — 相対する存在の性癖を可視化するスターター。
/// 性癖に刺さる行動で相手に破滅を付与できる。
/// エンシェント強化先は <see cref="FetishAbyss"/>。
/// </summary>
public class FetishBog : HypnosisCreatorRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicModel GetUpgradeReplacement() => ModelDb.Relic<FetishAbyss>();

    public override async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (Owner == null) return;
        if (!creature.IsEnemy) return;

        Flash();
        var rng = Owner.RunState.Rng.CombatOrbGeneration;
        // 性癖パワーの付与（表示待ち込み）まで待つ。投げっぱなしだと完了時刻が端末の早送り設定で変わり、共闘で同期がずれる。
        await EnemyFetishSlots.EnsureSpawnDefaultsAsync(new ThrowingPlayerChoiceContext(), creature, Owner, rng);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        FetishOrbHud.ClearAll();
        await Task.CompletedTask;
    }
}
