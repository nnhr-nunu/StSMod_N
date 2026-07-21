using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>双尾のネズミの心臓 — 希少。25ゴールド。</summary>
public class TwinTailRatHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "TWO_TAILED_RAT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 25);
}
