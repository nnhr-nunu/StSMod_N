using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ビャードニスの心臓 — 希少。縄張り意識を得る。</summary>
public class ByrdnisHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "BYRDONIS";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<TerritorialPower>(
            choiceContext, player.Creature, 1, player.Creature, null!);
        MarkUsed();
    }
}
