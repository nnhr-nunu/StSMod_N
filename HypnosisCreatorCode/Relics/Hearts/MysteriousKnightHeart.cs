using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>謎の騎士の心臓 — 希少。筋力3とプレート3。</summary>
public class MysteriousKnightHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "MYSTERIOUS_KNIGHT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, player.Creature, 3, player.Creature, null!);
        await PowerCmd.Apply<PlatingPower>(
            choiceContext, player.Creature, 3, player.Creature, null!);
        MarkUsed();
    }
}
