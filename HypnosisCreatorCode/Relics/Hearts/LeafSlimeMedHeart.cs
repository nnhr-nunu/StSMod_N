using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>リーフスライム(中)の心臓 — 非希少。最大HP+2。</summary>
public class LeafSlimeMedHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => false;
    public override string MonsterIdEntry => "LEAF_SLIME_MED";

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        Task.CompletedTask;

    protected override async Task OnPassiveObtain()
    {
        if (Owner == null) return;
        Flash();
        await CreatureCmd.GainMaxHp(Owner.Creature, 2);
    }
}
