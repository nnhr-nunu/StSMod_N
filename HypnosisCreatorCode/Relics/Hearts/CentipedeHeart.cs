using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>ムカデの心臓 — 希少。致死時1回HP25で蘇生（リザードテイル相当）。</summary>
public class CentipedeHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "CENTIPEDE";

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
        MarkUsed();
    }
}
