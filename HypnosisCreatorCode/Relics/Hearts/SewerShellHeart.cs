using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>下水道貝の心臓 — 希少。プレート4。</summary>
public class SewerShellHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SEWER_CLAM";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<PlatingPower>(this, choiceContext, player, 4);
}
