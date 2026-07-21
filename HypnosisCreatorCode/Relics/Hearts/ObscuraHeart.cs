using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>オブスキュラの心臓 — 希少。ランダム敵に16ダメージ。</summary>
public class ObscuraHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "THE_OBSCURA";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyDamage(this, choiceContext, player, 16);
}
