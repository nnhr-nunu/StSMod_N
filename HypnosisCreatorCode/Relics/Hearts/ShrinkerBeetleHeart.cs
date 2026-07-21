using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Potions;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>縮小甲虫の心臓 — 希少。ビートルジュースを入手。</summary>
public class ShrinkerBeetleHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SHRINKER_BEETLE";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRarePotion<BeetleJuice>(this, player);
}
