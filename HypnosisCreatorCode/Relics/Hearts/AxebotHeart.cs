using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// アックスマシン（Axebot）の心臓 — 希少。「アックスマシン」バフ（HP1踏みとどまり×2、重ねで+2）。
/// </summary>
public class AxebotHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "AXEBOT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<AxebotSurvivePower>(
            choiceContext, player.Creature, 2, player.Creature, null!);
        MarkUsed();
    }
}
