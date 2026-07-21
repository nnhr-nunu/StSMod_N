using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>チョンパーの心臓 — 希少。アーティファクト+2。</summary>
public class ChomperHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "CHOMPER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<ArtifactPower>(this, choiceContext, player, 2);
}
