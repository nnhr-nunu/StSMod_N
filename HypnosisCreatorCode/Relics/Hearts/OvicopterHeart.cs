using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// オビコプターの心臓 — 希少。所有心臓1つにつき2ブロック（右クリック）。
/// </summary>
public class OvicopterHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "OVICOPTER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var hearts = HeartInventory.CountHearts(player);
        if (hearts <= 0) return;

        Flash();
        await CreatureCmd.GainBlock(
            player.Creature,
            HeartActivationHelpers.BlockAmountWithDexterity(player.Creature, hearts * 2m),
            ValueProp.Unpowered,
            null);
        MarkUsed();
    }
}
