using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>湿カルティストの心臓 — 希少。儀式+1。</summary>
public class WetCultistHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "DAMP_CULTIST";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<RitualPower>(this, choiceContext, player, 1);
}
