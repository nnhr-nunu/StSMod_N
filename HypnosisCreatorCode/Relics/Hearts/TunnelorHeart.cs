using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>トンネラーの心臓 — 希少。ブロック+32。</summary>
public class TunnelorHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "TUNNELER";

    protected override decimal PreviewBlock => 32;


    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfBlock(this, choiceContext, player, DynamicVars.Block.BaseValue);
}
