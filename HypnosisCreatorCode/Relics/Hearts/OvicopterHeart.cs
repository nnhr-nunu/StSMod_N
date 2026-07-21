using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// オビコプターの心臓 — 戦闘開始時、所持心臓レリック1つにつき2ブロック（奪った心臓と同様の開幕ブロック）。
/// </summary>
public class OvicopterHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => false;
    public override string MonsterIdEntry => "OVICOPTER";

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player) =>
        Task.CompletedTask;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        await base.AfterPlayerTurnStart(choiceContext, player);
        if (player != Owner) return;
        if (Owner.PlayerCombatState?.TurnNumber != 1) return;

        var hearts = HeartInventory.CountHearts(Owner);
        if (hearts <= 0) return;

        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, hearts * 2m, ValueProp.Move, null);
    }
}
