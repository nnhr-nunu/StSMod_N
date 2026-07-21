using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>リーフスライム(中)の心臓 — 希少。最大HP+2。</summary>
public class LeafSlimeMedHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "LEAF_SLIME_M";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfMaxHp(this, choiceContext, player, 2);
}
