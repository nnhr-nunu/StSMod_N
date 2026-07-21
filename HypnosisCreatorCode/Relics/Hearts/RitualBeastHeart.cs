using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>儀式の獣の心臓 — 希少。最大HP+10。</summary>
public class RitualBeastHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "CEREMONIAL_BEAST";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfMaxHp(this, choiceContext, player, 10);
}
