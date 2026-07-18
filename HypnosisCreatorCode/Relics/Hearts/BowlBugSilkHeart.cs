using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ボウル虫(絹)の心臓 — 希少。ランダム敵に脆弱+1。</summary>
public class BowlBugSilkHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "BOWL_BUG_SILK";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyPower<VulnerablePower>(
            this, choiceContext, player, 1);
}
