using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>フクロウ判事の心臓 — 希少。このターン、本家飛翔（Soar）と同じ被ダメ半減を得る。</summary>
public class OwlMagistrateHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "OWL_MAGISTRATE";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<ThisTurnSoarPower>(
            choiceContext, player.Creature, 1m, player.Creature, null!);
        MarkUsed();
    }
}
