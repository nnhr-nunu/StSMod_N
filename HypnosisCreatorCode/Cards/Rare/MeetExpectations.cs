using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>期待に応えて — 手札のカウントカードの解決後コスト合計の2乗ダメージを与え、それらのコストを1下げる。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MeetExpectations() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(0M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var hand = Owner.PlayerCombatState?.Hand;
        var countCards = hand?.Cards.Where(c => c != this && CountRules.HasCountKeyword(c)).ToList()
            ?? [];
        var sumCost = countCards.Sum(c => c.EnergyCost.GetResolved());

        DynamicVars.Damage.BaseValue = sumCost * sumCost;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);

        foreach (var card in countCards)
            card.EnergyCost.AddThisCombat(-1);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
