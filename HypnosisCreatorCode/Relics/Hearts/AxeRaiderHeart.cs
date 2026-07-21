using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>斧レイダーの心臓 — 希少。ブロック+5。</summary>
public class AxeRaiderHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "AXE_RUBY_RAIDER";

    protected override decimal PreviewBlock => 5;


    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfBlock(this, choiceContext, player, DynamicVars.Block.BaseValue);
}
