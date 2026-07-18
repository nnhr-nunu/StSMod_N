using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>幽霊船の心臓 — 非希少。ゴールド75。</summary>
public class GhostShipHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => false;
    public override string MonsterIdEntry => "GHOST_SHIP";

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        Task.CompletedTask;

    protected override async Task OnPassiveObtain() =>
        await HeartActivationHelpers.PassiveGold(this, 75);
}
