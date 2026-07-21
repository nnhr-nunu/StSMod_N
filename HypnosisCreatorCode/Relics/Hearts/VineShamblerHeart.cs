using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ヴァインシャンブラーの心臓 — 希少。25ゴールド。</summary>
public class VineShamblerHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "VINE_SHAMBLER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 25);
}
