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

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// トランスに溶けゆく — 対象へ付与したトランス合計回数×係数のダメージ。
/// 合計は {CalculatedDamage:diff()} で枠・説明・緑表示を統一。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MeltIntoTrance() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0M),
        new ExtraDamageVar(15M),
        new DynamicVar("PerTrance", 15M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(FallenTranceMultiplier)
    ];

    internal static decimal ComputeDamage(CardModel card, Creature? target)
    {
        var fallen = target != null ? TranceFallTracker.Get(target) : 0;
        return fallen * card.DynamicVars.ExtraDamage.BaseValue;
    }

    private static decimal FallenTranceMultiplier(CardModel card, Creature? target)
    {
        if (target == null) return 0M;
        return TranceFallTracker.Get(target);
    }

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(c => TranceFallTracker.Get(c) > 0);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(5M);
        DynamicVars["PerTrance"].UpgradeValueBy(5M);
    }
}
