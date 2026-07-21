using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;

/// <summary>
/// マギ・ナイトの心臓 — 希少。山札をすべてアップグレード（戦闘中のみ／心臓発動自体が戦闘中）。
/// </summary>
public class MagiKnightHeart : EnemyHeartRelic
{
    public override string MonsterIdEntry => "MAGI_KNIGHT";

    public override Task ActivateAsync(PlayerChoiceContext choiceContext, Player player)
    {
        var draw = player.PlayerCombatState?.DrawPile;
        if (draw == null) return Task.CompletedTask;

        var cards = draw.Cards.Where(c => c.IsUpgradable).ToList();
        if (cards.Count == 0) return Task.CompletedTask;

        Flash();
        CardCmd.Upgrade(cards, CardPreviewStyle.None);
        MarkUsed();
        return Task.CompletedTask;
    }
}
