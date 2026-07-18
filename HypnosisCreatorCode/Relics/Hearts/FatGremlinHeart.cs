using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>太っちょグレmlinの心臓 — 非希少。ゴールド25。</summary>
public class FatGremlinHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => false;
    public override string MonsterIdEntry => "FAT_GREMLIN";

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        Task.CompletedTask;

    protected override async Task OnPassiveObtain() =>
        await HeartActivationHelpers.PassiveGold(this, 25);
}
