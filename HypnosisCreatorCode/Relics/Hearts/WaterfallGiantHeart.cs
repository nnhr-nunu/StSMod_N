using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>滝の巨人の心臓 — 希少。100ゴールド。</summary>
public class WaterfallGiantHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "WATERFALL_GIANT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 100);
}
