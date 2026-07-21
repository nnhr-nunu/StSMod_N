using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>クイーンの心臓 — 希少。全敵に脆弱99・脱力99・弱体99。</summary>
public class QueenHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "QUEEN";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var combat = player.Creature.CombatState;
        if (combat == null) return;

        var enemies = combat.HittableEnemies.ToList();
        if (enemies.Count == 0) return;

        Flash();
        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, 99, player.Creature, null!);
            await PowerCmd.Apply<FrailPower>(choiceContext, enemy, 99, player.Creature, null!);
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, 99, player.Creature, null!);
        }
        MarkUsed();
    }
}
