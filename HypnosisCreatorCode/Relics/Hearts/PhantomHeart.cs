using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ファントムの心臓 — 希少。滑り+2。</summary>
public class PhantomHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "VANTOM";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<SlipperyPower>(this, choiceContext, player, 2);
}
