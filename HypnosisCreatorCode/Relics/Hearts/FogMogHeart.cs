using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>フォグモグの心臓 — 希少。最大HP+4。</summary>
public class FogMogHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "FOGMOG";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfMaxHp(this, choiceContext, player, 4);
}
