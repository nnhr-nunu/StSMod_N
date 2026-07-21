using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>マイトの心臓 — 希少。ランダムな相手に毒5を2回付与。</summary>
public class MyteHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "MYTE";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyPowerTimes<PoisonPower>(
            this, choiceContext, player, 5, 2);
}
