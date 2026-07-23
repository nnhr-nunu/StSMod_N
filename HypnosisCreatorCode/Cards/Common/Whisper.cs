using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>
/// 囁き — 山札の性癖カードを選び、その性癖を対象に目覚めさせる。廃棄。
/// 対象がすでに持つ性癖だけのカードは選べない。候補がなければ廃棄のみ。
/// UG: 2枚選び、このカードは必ず性癖にささる。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Whisper() : HypnosisCreatorCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    /// <summary>UG時のみ必中刺さり。</summary>
    public override bool AlwaysHitsFetish => IsUpgraded;

    protected override bool ShouldGlowWhenConditionMet()
    {
        var draw = Owner.PlayerCombatState?.DrawPile;
        if (draw == null) return false;

        return GlowIfTargetOrAnyEnemy(target =>
            draw.Cards.Any(c => IsCandidate(c, target)));
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    private static bool IsCandidate(CardModel c, Creature target) =>
        CardFetishLookup.GetFetishes(c).Any(f => !FetishCombat.HasFetish(target, f));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var count = DynamicVars.Cards.IntValue;
        var combat = Owner.PlayerCombatState;
        if (combat == null) return;

        var target = play.Target;
        bool IsSelectable(CardModel c) => IsCandidate(c, target);

        var drawCandidates = combat.DrawPile.Cards
            .Where(IsSelectable)
            .ToList();

        if (drawCandidates.Count == 0) return;

        IReadOnlyList<CardModel> selected;
        try
        {
            selected = (await CardSelectCmd.FromCombatPile(
                choiceContext,
                combat.DrawPile,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, count),
                IsSelectable)).ToList();
        }
        catch
        {
            selected = drawCandidates
                .OrderBy(_ => Guid.NewGuid())
                .Take(count)
                .ToList();
        }

        if (selected.Count == 0)
        {
            selected = drawCandidates
                .OrderBy(_ => Guid.NewGuid())
                .Take(count)
                .ToList();
        }

        var awakened = new List<FetishType>();
        foreach (var card in selected)
        {
            foreach (var fetish in CardFetishLookup.GetFetishes(card))
            {
                FetishCombat.Awaken(play.Target, fetish, Owner);
                awakened.Add(fetish);
            }
        }

        // UG: 選んだカードの性癖タグで必ず刺さる（感度3000倍と同様に1プレイ1回）
        if (IsUpgraded && awakened.Count > 0)
        {
            await FetishCombat.TryFetishHit(
                choiceContext,
                play.Target,
                Owner.Creature,
                this,
                awakened.Distinct().ToList(),
                alwaysHit: true,
                singleHit: true);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1M);
}
