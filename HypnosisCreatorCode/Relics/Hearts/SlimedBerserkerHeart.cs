using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// スライムバーサーカーの心臓 — 希少。
/// 相手へプレイ可能な粘液（状態異常）を5枚手札に加える。
/// </summary>
public class SlimedBerserkerHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SLIMED_BERSERKER";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature.CombatState == null) return;

        Flash();
        var combat = player.Creature.CombatState;
        for (var i = 0; i < 5; i++)
        {
            var slime = combat.CreateCard(ModelDb.Card<AbnormalSlime>(), player);
            if (slime is AbnormalSlime ab)
                ab.FreeEnemyPlay = true;
            await CardPileCmd.AddGeneratedCardToCombat(slime, PileType.Hand, player);
        }

        MarkUsed();
    }
}
