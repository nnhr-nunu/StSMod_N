using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// 蠢く群生体の心臓 — 希少。1ターンのHP損失上限（硬い殻同趣旨）。
/// 重ねがけで上限半減: 20→10→5…（最低1）。
/// </summary>
public class SwarmingHiveHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SKULKING_COLONY";

    /// <summary>旧仮キー SWARMING_HIVE も受け付ける。</summary>
    public override IReadOnlyList<string> MonsterIdEntries =>
        ["SKULKING_COLONY", "SWARMING_HIVE"];

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        var creature = player.Creature;
        var shell = creature.GetPower<SwarmingHiveShellPower>();
        if (shell != null)
        {
            await shell.StackFromHeartActivation(choiceContext, creature);
            MarkUsed();
            return;
        }

        var legacy = creature.GetPower<HardenedShellPower>();
        var applications = 1;
        if (legacy != null)
        {
            applications = Math.Max(1, (int)Math.Round(legacy.Amount / 20m));
            await PowerCmd.Remove(legacy);
        }

        await PowerCmd.Apply<SwarmingHiveShellPower>(
            choiceContext, creature, SwarmingHiveShellPower.CapForApplications(applications),
            creature, null!);

        await creature.GetPower<SwarmingHiveShellPower>()!
            .SyncApplications(choiceContext, creature, applications);
        MarkUsed();
    }
}
