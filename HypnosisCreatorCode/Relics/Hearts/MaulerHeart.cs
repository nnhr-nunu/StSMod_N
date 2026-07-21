using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>モーラーの心臓 — 希少。ランダム敵に弱体+2。</summary>
public class MaulerHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "MAWLER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyPower<WeakPower>(this, choiceContext, player, 2);
}
