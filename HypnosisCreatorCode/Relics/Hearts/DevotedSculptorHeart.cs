using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>狂信の彫刻家の心臓 — 希少。儀式2。</summary>
public class DevotedSculptorHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "DEVOTED_SCULPTOR";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<RitualPower>(this, choiceContext, player, 2);
}
