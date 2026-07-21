using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>追跡レイダーの心臓 — 希少。ランダム敵に脆弱+2。</summary>
public class TrackerRaiderHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "TRACKER_RUBY_RAIDER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyPower<VulnerablePower>(
            this, choiceContext, player, 2);
}
