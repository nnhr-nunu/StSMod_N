using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>化石ストーカーの心臓 — 希少。吸血1。</summary>
public class FossilStalkerHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "FOSSIL_STALKER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<DevourLifePower>(this, choiceContext, player, 1);
}
