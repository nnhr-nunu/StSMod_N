using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>血の司祭の心臓 — 非希少。最大HP+5、ゴールド50。</summary>
public class BloodPriestHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => false;
    public override string MonsterIdEntry => "BLOOD_PRIEST";

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        Task.CompletedTask;

    protected override async Task OnPassiveObtain()
    {
        await HeartActivationHelpers.PassiveMaxHp(this, 5);
        await HeartActivationHelpers.PassiveGold(this, 50);
    }
}
