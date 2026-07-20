using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>万足ムカデの心臓 — 死亡時1回 HP25 で復活（CSV: 非希少・パッシブ）。</summary>
public class CentipedeHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => false;
    public override string MonsterIdEntry => "DECIMILLIPEDE_SEGMENT_FRONT";

    // 式本体にする（HeartRegistry の未初期化サンプルでフィールド初期化が走らないため）
    public override IReadOnlyList<string> MonsterIdEntries =>
    [
        "DECIMILLIPEDE_SEGMENT_FRONT",
        "DECIMILLIPEDE_SEGMENT_MIDDLE",
        "DECIMILLIPEDE_SEGMENT_BACK"
    ];

    public override bool IsUsedUp => WasUsed;

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        Task.CompletedTask;

    public override bool ShouldDieLate(Creature creature)
    {
        if (Owner == null || creature != Owner.Creature) return true;
        return WasUsed;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (Owner == null || creature != Owner.Creature) return;
        Flash();
        await CreatureCmd.Heal(creature, 25);
        WasUsed = true;
    }
}
