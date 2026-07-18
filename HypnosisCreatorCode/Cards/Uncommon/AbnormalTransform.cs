using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// アブノーマル — 支援カードのためカウントキーワードは無い（mechanics-lock.md）。
/// 手札のカードを最大2枚、ランダムなアブノーマル性癖カードへこの戦闘中だけ変換する（コスト0）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AbnormalTransform() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(2)];

    private static bool IsAbnormalPoolCard(CardModel c) =>
        c is HypnosisCreatorCard { Rarity: not CardRarity.Token } hc &&
        hc.CardFetishes.Contains(FetishType.Abnormal) &&
        c.GetType() != typeof(AbnormalTransform);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = Owner.PlayerCombatState?.Hand;
        if (hand == null || CombatState == null) return;

        var pool = ModelDb.AllCards.Where(IsAbnormalPoolCard).ToList();
        if (pool.Count == 0) return;

        var count = Math.Min(DynamicVars.Cards.IntValue, hand.Cards.Count(c => c != this));
        if (count <= 0) return;

        IReadOnlyList<CardModel> selected;
        try
        {
            selected = (await CardSelectCmd.FromCombatPile(
                choiceContext, hand, Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, count),
                c => c != this)).ToList();
        }
        catch
        {
            selected = hand.Cards.Where(c => c != this)
                .OrderBy(_ => Guid.NewGuid()).Take(count).ToList();
        }

        var rng = Owner.RunState.Rng.CombatCardSelection;
        foreach (var card in selected)
        {
            var canonical = pool[rng.NextInt(pool.Count)];
            var result = await CardCmd.Transform(card, CombatState.CreateCard(canonical, Owner));
            result?.cardAdded.EnergyCost.SetThisCombat(0);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1M);
}
