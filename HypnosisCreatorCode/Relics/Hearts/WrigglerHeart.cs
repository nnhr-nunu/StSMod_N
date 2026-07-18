using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>リグラーの心臓 — 希少。手札のランダム攻撃に鋭利+1（CSV: Parasite +3 dmg）。</summary>
public class WrigglerHeart : EnemyHeartRelic
{
    public override bool IsRareHeart => true;
    public override string MonsterIdEntry => "WRIGGLER";

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var hand = player.PlayerCombatState?.Hand;
        if (hand == null) return Task.CompletedTask;

        var attacks = hand.Cards.Where(c => c.Type == CardType.Attack).ToList();
        if (attacks.Count == 0) return Task.CompletedTask;

        var rng = player.RunState.Rng.CombatCardSelection;
        var card = attacks[rng.NextInt(attacks.Count)];

        Flash();
        CardCmd.Enchant<Sharp>(card, 1);
        MarkUsed();
        return Task.CompletedTask;
    }
}
