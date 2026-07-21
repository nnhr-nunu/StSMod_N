using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>タレットオペレーターの心臓 — 希少。2×5ダメージ（筋力反映）。</summary>
public class TurretOperatorHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "TURRET_OPERATOR";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyDamage(this, choiceContext, player, 2, 5);
}
