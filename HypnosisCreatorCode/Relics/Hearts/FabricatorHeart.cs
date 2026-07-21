using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// ファブリケーターの心臓 — 希少。
/// ザップマシン（Zapbot）を味方ペットとして召喚し、以降プレイヤーターン終了時にランダム敵を攻撃する。
/// </summary>
public class FabricatorHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "FABRICATOR";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature.CombatState == null) return;

        Flash();
        var zapbot = await PlayerCmd.AddPet<Zapbot>(player);
        await PowerCmd.Apply<AllyZapbotPower>(
            choiceContext, zapbot, 1, player.Creature, null!);

        // 召喚直後に1回攻撃。以降は AllyZapbotPower がターン終了時に攻撃する。
        await AllyZapbotAttacks.Perform(choiceContext, zapbot);
        MarkUsed();
    }
}
