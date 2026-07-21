using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ソウルフィッシュの心臓 — 希少。霊体1。</summary>
public class SoulFishHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SOUL_FYSH";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<IntangiblePower>(this, choiceContext, player, 1);
}
