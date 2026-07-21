using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>血族の司祭の心臓 — 希少。最大HP+5、ゴールド50。</summary>
public class BloodPriestHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "KIN_PRIEST";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareMaxHpAndGold(this, choiceContext, player, 5, 50);
}
