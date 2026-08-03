using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
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

    protected override bool ShouldGlowWhenConditionMet()
    {
        var draw = Owner.PlayerCombatState?.DrawPile;
        return draw != null && draw.Cards.Any(CountRules.HasCountKeyword);
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var combat = Owner.PlayerCombatState;
        var draw = combat?.DrawPile;
        if (draw == null) return Task.CompletedTask;

        var candidates = draw.Cards.Where(CountRules.HasCountKeyword).ToList();
        if (candidates.Count == 0) return Task.CompletedTask;

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

        // プレイ中の即時手札移動は飛来演出と競合する。Good! 同様に遅延キューへ（演出なしでも可）。
        PendingHandCardAdd.EnqueueExisting(selected, Owner, this, CardPilePosition.Top);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
