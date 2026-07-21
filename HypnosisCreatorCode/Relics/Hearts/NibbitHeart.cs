using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ニビットの心臓 — 希少。ブロック+5。</summary>
public class NibbitHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "NIBBIT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfBlock(this, choiceContext, player, 5);
}
