using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>スラッジスピナーの心臓 — 希少。ランダム敵に脱力+1。</summary>
public class SludgeSpinnerHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SLUDGE_SPINNER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareRandomEnemyPower<WeakPower>(this, choiceContext, player, 1);
}
