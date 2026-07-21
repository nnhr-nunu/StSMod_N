using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ラガヴーリン族長の心臓 — 希少。プレート6。</summary>
public class LagavulinMatriarchHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "LAGAVULIN_MATRIARCH";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<PlatingPower>(this, choiceContext, player, 6);
}
