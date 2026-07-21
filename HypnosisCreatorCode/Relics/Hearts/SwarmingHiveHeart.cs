using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>蠢く群生体の心臓 — 希少。戦闘中、1ターンに20以上のHPを失わない（硬い殻／エリート同効果）。</summary>
public class SwarmingHiveHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SKULKING_COLONY";

    /// <summary>旧仮キー SWARMING_HIVE も受け付ける。</summary>
    public override IReadOnlyList<string> MonsterIdEntries =>
        ["SKULKING_COLONY", "SWARMING_HIVE"];

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<HardenedShellPower>(this, choiceContext, player, 20);
}
