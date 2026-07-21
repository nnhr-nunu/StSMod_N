using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ボウル虫(卵)の心臓 — 希少。ブロック+7。</summary>
public class BowlBugEggHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "BOWLBUG_EGG";

    protected override decimal PreviewBlock => 7;


    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfBlock(this, choiceContext, player, DynamicVars.Block.BaseValue);
}
