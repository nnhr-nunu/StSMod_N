using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>幻虫の心臓 — 希少。ランダム敵に弱体+6（CSV: Phobia/Fear 6）。</summary>
public class PhantasmalBugHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "PHANTASMAL_BUG";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyPower<WeakPower>(this, choiceContext, player, 6);
}
