using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>細枝スライム(小)の心臓 — 希少。0コスト粘液を手札へ。</summary>
public class TwigSlimeSmallHeart : EnemyHeartRelic
{
    // 細枝スライム小
    public override string MonsterIdEntry => "TWIG_SLIME_S";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareZeroCostSlimed(this, choiceContext, player);
}
