using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>忘れられし者の心臓 — 希少。敵の敏捷-2、自分の敏捷+2。</summary>
public class TheForgottenHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "THE_FORGOTTEN";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var enemy = HeartActivationHelpers.PickRandomEnemy(player);
        if (enemy == null) return;

        Flash();
        await PowerCmd.Apply<DexterityPower>(choiceContext, enemy, -2, player.Creature, null!);
        await PowerCmd.Apply<DexterityPower>(choiceContext, player.Creature, 2, player.Creature, null!);
        MarkUsed();
    }
}
