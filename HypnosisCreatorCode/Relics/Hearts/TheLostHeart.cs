using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>迷える者の心臓 — 希少。敵の筋力-2、自分の筋力+2。</summary>
public class TheLostHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "THE_LOST";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var enemy = HeartActivationHelpers.PickRandomEnemy(player);
        if (enemy == null) return;

        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, enemy, -2, player.Creature, null!);
        await PowerCmd.Apply<StrengthPower>(choiceContext, player.Creature, 2, player.Creature, null!);
        MarkUsed();
    }
}
