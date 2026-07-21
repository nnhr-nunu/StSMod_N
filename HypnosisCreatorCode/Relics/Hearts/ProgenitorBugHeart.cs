using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>原虫の心臓 — 希少。ブロック+14。</summary>
public class ProgenitorBugHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "LOUSE_PROGENITOR";

    protected override decimal PreviewBlock => 14;


    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfBlock(this, choiceContext, player, DynamicVars.Block.BaseValue);
}
