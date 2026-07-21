using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>クロスボウレイダーの心臓 — 希少。ランダム敵に14ダメージ。</summary>
public class CrossbowRaiderHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "CROSSBOW_RUBY_RAIDER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyDamage(this, choiceContext, player, 14);
}
