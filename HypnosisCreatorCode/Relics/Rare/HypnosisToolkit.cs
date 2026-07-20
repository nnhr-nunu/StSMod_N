using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Relics.Rare;

/// <summary>
/// 催眠七つ道具 — 専用レアレリック。
/// 戦闘開始時（通常ドロー前）に、山札のカウントカードを最大2枚手札へ加える。
/// </summary>
public class HypnosisToolkit : HypnosisCreatorRelic
{
    private const int DrawCount = 2;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task BeforeHandDraw(
        Player player,
        PlayerChoiceContext choiceContext,
        ICombatState combatState)
    {
        if (player != Owner || Owner == null) return;
        if (Owner.PlayerCombatState?.TurnNumber != 1) return;

        var drawPile = Owner.PlayerCombatState.DrawPile;
        if (drawPile == null || drawPile.Cards.Count == 0) return;

        var candidates = drawPile.Cards.Where(CountRules.HasCountKeyword).ToList();
        if (candidates.Count == 0) return;

        var picks = PickRandom(candidates, DrawCount, Owner);
        if (picks.Count == 0) return;

        Flash();
        await CardPileCmd.Add(picks, PileType.Hand, CardPilePosition.Top, this);
    }

    private static List<CardModel> PickRandom(List<CardModel> pool, int count, Player owner)
    {
        var remaining = pool.ToList();
        var picks = new List<CardModel>(count);
        var rng = owner.RunState.Rng.CombatCardSelection;
        while (picks.Count < count && remaining.Count > 0)
        {
            var index = rng.NextInt(remaining.Count);
            picks.Add(remaining[index]);
            remaining.RemoveAt(index);
        }

        return picks;
    }
}
