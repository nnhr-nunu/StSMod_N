using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ファットグレムリンの心臓 — 希少。25ゴールド。</summary>
public class FatGremlinHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "FAT_GREMLIN";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 25);
}
