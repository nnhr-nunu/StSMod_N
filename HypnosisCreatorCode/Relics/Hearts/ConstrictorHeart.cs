using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>絞蛇の心臓 — 希少。ランダム敵に絞め付け+3。</summary>
public class ConstrictorHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SLITHERING_STRANGLER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var enemy = HeartActivationHelpers.PickRandomEnemy(player);
        if (enemy == null) return;

        Flash();
        await PowerCmd.Apply<ConstrictPower>(choiceContext, enemy, 3, player.Creature, null!);
        MarkUsed();
    }
}
