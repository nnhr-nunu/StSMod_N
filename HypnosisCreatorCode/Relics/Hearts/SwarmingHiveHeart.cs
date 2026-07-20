using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>蠢く群生体の心臓 — 希少。このターンに20以上のHPを失わない。</summary>
public class SwarmingHiveHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "SKULKING_COLONY";

    /// <summary>旧仮キー SWARMING_HIVE も受け付ける。</summary>
    public override IReadOnlyList<string> MonsterIdEntries { get; } =
        ["SKULKING_COLONY", "SWARMING_HIVE"];

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfPower<HardToKillPower>(this, choiceContext, player, 20);
}
