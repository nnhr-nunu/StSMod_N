using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>リビングシールドの心臓 — 希少。プレート4。</summary>
public class LivingShieldHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "LIVING_SHIELD";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<HardenedShellPower>(this, choiceContext, player, 4);
}
