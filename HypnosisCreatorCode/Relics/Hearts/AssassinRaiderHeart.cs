using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>暗殺レイダーの心臓 — 希少。ランダム敵に10ダメージ。</summary>
public class AssassinRaiderHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "ASSASSIN_RUBY_RAIDER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyDamage(this, choiceContext, player, 10);
}
