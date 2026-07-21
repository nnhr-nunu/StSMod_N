using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>偽商人の心臓 — 希少。333ゴールド。</summary>
public class FakeMerchantHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "FAKE_MERCHANT_MONSTER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 333);
}
