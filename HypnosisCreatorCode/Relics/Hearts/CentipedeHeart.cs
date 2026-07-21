using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// 万足ムカデの心臓 — 希少。「万足ムカデ」バフを得る（死亡時 HP25 復活・重ねがけ可）。
/// </summary>
public class CentipedeHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "DECIMILLIPEDE_SEGMENT_FRONT";

    public override IReadOnlyList<string> MonsterIdEntries =>
    [
        "DECIMILLIPEDE_SEGMENT_FRONT",
        "DECIMILLIPEDE_SEGMENT_MIDDLE",
        "DECIMILLIPEDE_SEGMENT_BACK"
    ];

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<CentipedeRevivePower>(
            choiceContext, player.Creature, 1, player.Creature, null!);
        MarkUsed();
    }
}
