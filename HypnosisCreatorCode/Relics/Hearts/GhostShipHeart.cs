using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>幽霊船の心臓 — 希少。55ゴールド。</summary>
public class GhostShipHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "HAUNTED_SHIP";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 55);
}
