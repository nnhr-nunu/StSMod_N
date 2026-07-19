using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 術式の開示 — 天賦・廃棄。山札からカウント2枚を手札へ（1枚は性癖優先）。UG0コスト。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class RitualReveal() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Innate, CardKeyword.Exhaust];

    private const int PullCount = 2;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var combat = Owner.PlayerCombatState;
        var draw = combat?.DrawPile;
        if (draw == null) return;

        var candidates = draw.Cards.Where(CountRules.HasCountKeyword).ToList();
        if (candidates.Count == 0) return;

        var rng = Owner.RunState.Rng.CombatCardSelection;
        var targetFetishes = FetishCombat.GetFetishes(play.Target).ToHashSet();
        var selected = new List<CardModel>();

        var fetishMatch = candidates
            .Where(c => CardFetishLookup.GetFetishes(c).Any(f => targetFetishes.Contains(f)))
            .ToList();
        if (fetishMatch.Count > 0)
            selected.Add(fetishMatch[rng.NextInt(fetishMatch.Count)]);

        var remaining = candidates.Where(c => !selected.Contains(c)).ToList();
        while (selected.Count < PullCount && remaining.Count > 0)
        {
            var pick = remaining[rng.NextInt(remaining.Count)];
            selected.Add(pick);
            remaining.Remove(pick);
        }

        foreach (var card in selected)
            await CardPileCmd.Add(card, PileType.Hand, CardPilePosition.Top, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
