using BaseLib.Patches.Localization;
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
/// 期待に応えて — 手札のカウントカードの解決後コスト合計+1 の2乗ダメージを与え、それらのコストを1下げる。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MeetExpectations() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    static MeetExpectations()
    {
        DescriptionOverrides.CustomizeDescriptionPost += PrependCombatDamage;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0M),
        new ExtraDamageVar(1M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalcSquaredHandCountCost)
    ];

    internal static decimal ComputeDamage(CardModel card) => CalcSquaredHandCountCost(card, null);

    private static decimal CalcSquaredHandCountCost(CardModel card, Creature? target)
    {
        _ = target;
        var player = card.Owner;
        if (player == null) return 0M;
        var sum = CountRules.SumResolvedCountCostsInHand(player, exclude: card);
        var n = sum + 1;
        return n * n;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

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

    private static void PrependCombatDamage(CardModel card, Creature? target, ref string description)
    {
        if (card is not MeetExpectations meet) return;
        if (!CombatPreviewText.IsActive(meet)) return;

        var raw = ComputeDamage(meet);
        var previewTarget = target ?? meet.CurrentTarget;
        var preview = CardDamagePreview.ApplyModifiers(meet, previewTarget, raw, ValueProp.Move);
        var formatted = CombatPreviewText.FormatPreviewAmount(preview, raw);

        var prefix = UpgradeCardText.IsJapaneseUi()
            ? $"{formatted}ダメージを与える。"
            : $"Deal {formatted} damage. ";
        description = prefix + description;
    }
}
