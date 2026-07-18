using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>エントマンサーの心臓 — 希少。ランダム敵に3ダメージ×7。</summary>
public class EntmancerHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "ENTMANCER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyDamage(this, choiceContext, player, 3, 7);
}
