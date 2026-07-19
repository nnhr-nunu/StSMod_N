using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// アブノーマル — 手札を最大X枚、備考の他色アブノーマルカードへ変化させ、このターンコスト0にする。
/// UGではアップグレード済みの変換先になる。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AbnormalTransform() : HypnosisCreatorCard(-1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override bool HasEnergyCostX => true;

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = Owner.PlayerCombatState?.Hand;
        if (hand == null || CombatState == null) return;

        var pool = AbnormalOtherColorPool.GetCanonicalCards();
        if (pool.Count == 0) return;

        var x = Math.Max(0, ResolveEnergyXValue());
        var count = Math.Min(x, hand.Cards.Count(c => c != this));
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
            var generated = CombatState.CreateCard(canonical, Owner);
            if (IsUpgraded)
                CardCmd.Upgrade(generated);
            var result = await CardCmd.Transform(card, generated);
            // 仕様: このターン、コストなしでプレイできる
            result?.cardAdded.EnergyCost.SetThisTurn(0);
        }
    }

    protected override void OnUpgrade() { }
}
