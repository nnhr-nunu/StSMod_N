using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// カイザークラブの心臓 — 希少。最大HP+10。
/// 左右爪（CRUSHER / ROCKET）は同一効果。
/// </summary>
public class KaiserCrabHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "CRUSHER";

    public override IReadOnlyList<string> MonsterIdEntries { get; } =
        ["CRUSHER", "ROCKET"];

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        await HeartActivationHelpers.ActivateRareSelfMaxHp(this, choiceContext, player, 10);
}
