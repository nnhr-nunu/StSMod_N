using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>バードニスの心臓 — 希少。領土+1、失敗時は筋力+1（CSV: Territory/Byrd）。</summary>
public class ByrdnisHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "BYRDONIS";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        if (player.Creature.GetPowerAmount<TerritorialPower>() <= 0)
            await PowerCmd.Apply<TerritorialPower>(choiceContext, player.Creature, 1, player.Creature, null!);
        else
            await PowerCmd.Apply<StrengthPower>(choiceContext, player.Creature, 1, player.Creature, null!);
        MarkUsed();
    }
}
