using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>双尾ネズミの心臓 — 非希少。ゴールド25。</summary>
public class TwinTailRatHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => false;
    public override string MonsterIdEntry => "TWIN_TAIL_RAT";

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        Task.CompletedTask;

    protected override async Task OnPassiveObtain() =>
        await HeartActivationHelpers.PassiveGold(this, 25);
}
