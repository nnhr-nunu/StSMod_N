using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Potions;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ボウル虫(岩)の心臓 — 希少。岩形ポーションを入手。</summary>
public class BowlBugRockHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "BOWLBUG_ROCK";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRarePotion<PotionShapedRock>(this, player);
}
