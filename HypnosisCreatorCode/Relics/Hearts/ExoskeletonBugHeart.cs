using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// 外骨格蟲の心臓 — 希少。2ターン、不死身9（本家 HardToKill と同性能・敵ターン終了ごとに1消費）。
/// </summary>
public class ExoskeletonBugHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "EXOSKELETON";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        var creature = player.Creature;
        var existing = creature.GetPower<ExoskeletonBugBuffPower>();
        if (existing != null)
            await PowerCmd.Remove(existing);

        await PowerCmd.Apply<ExoskeletonBugBuffPower>(
            choiceContext, creature, 9m, creature, null!);
        MarkUsed();
    }
}
