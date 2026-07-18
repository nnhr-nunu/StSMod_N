using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Potions;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>眠り甲虫の心臓 — 希少。ビートルジュースを入手。</summary>
public class SleepingBeetleHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "SLEEPING_BEETLE";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRarePotion<BeetleJuice>(this, player);
}
