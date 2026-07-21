using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>スペクトルナイトの心臓 — 希少。手札をすべて廃棄。</summary>
public class SpectralKnightHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "SPECTRAL_KNIGHT";

    public override async Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var hand = player.PlayerCombatState?.Hand;
        if (hand == null) return;

        var cards = hand.Cards.ToList();
        if (cards.Count == 0) return;

        Flash();
        foreach (var card in cards)
            await CardCmd.Exhaust(choiceContext, card);
        MarkUsed();
    }
}
