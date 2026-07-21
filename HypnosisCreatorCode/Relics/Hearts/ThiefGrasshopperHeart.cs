using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>泥棒バッタの心臓 — 希少。50ゴールド。</summary>
public class ThiefGrasshopperHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "THIEVING_HOPPER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareGold(this, player, 50);
}
