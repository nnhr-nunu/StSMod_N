using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>フライコニドの心臓 — 希少。ランダム敵に脆弱+1・弱体+1。</summary>
public class FlyconidHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "FLYCONID";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var enemy = HeartActivationHelpers.PickRandomEnemy(player);
        if (enemy == null) return;

        Flash();
        await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, 1, player.Creature, null!);
        await PowerCmd.Apply<WeakPower>(choiceContext, enemy, 1, player.Creature, null!);
        MarkUsed();
    }
}
