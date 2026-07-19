using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 期待に応えて — 手札のカウントカードの解決後コスト合計の2乗ダメージを与え、それらのコストを1下げる。
/// ダメージ表示は本家 BodySlam / MindBlast と同じ <see cref="CalculatedDamageVar"/> でプレビューする。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MeetExpectations() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0M),
        new ExtraDamageVar(0M),
        // result = Func(card, target) + Base * Extra  （Base=Extra=0 なので Func の値がそのままダメージ）
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalcSquaredHandCountCost)
    ];

    /// <summary>静的必須（CalculatedVar.WithMultiplier の制約）。</summary>
    private static decimal CalcSquaredHandCountCost(CardModel card, Creature? target)
    {
        var player = card.Owner;
        if (player == null) return 0M;
        var sum = CountRules.SumResolvedCountCostsInHand(player, exclude: card);
        return sum * sum;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        // プレビューと同じ経路で算出されたダメージを使用
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);

        var countCards = CountRules.GetCountCardsInHand(Owner, exclude: this);
        foreach (var card in countCards)
            card.EnergyCost.AddThisCombat(-1);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
