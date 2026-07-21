using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>カエルの騎士の心臓 — 希少。プレート7。</summary>
public class FrogKnightHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "FROG_KNIGHT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<PlatingPower>(this, choiceContext, player, 7);
}
