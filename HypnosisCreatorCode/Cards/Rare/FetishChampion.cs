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
/// 性癖の覇者 — 対象の性癖数だけ20ダメージを与える（UG25）。
/// CSV備考: 性癖の数だけ攻撃し、性癖を刺す（最大4回）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FetishChampion() : HypnosisCreatorCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes =>
        [FetishType.Abnormal, FetishType.Sm, FetishType.DomSub];

    /// <summary>複数タグを種類ごとに刺す（最大3種。攻撃回数は対象性癖数で最大4）。</summary>
    public override bool? FetishHitPerTypeOverride => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20M, ValueProp.Move),
        new CalculationBaseVar(0M),
        new CalculationExtraVar(1M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalcTotalDamage),
        new CalculatedVar("HitCount").WithMultiplier(CalcHitCount)
    ];

    private static decimal CalcHitCount(CardModel card, Creature? target) =>
        target == null ? 0M : FetishCombat.GetFetishes(target).Count;

    private static decimal CalcTotalDamage(CardModel card, Creature? target) =>
        card.DynamicVars.Damage.BaseValue * CalcHitCount(card, target);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var hits = (int)CalcHitCount(this, play.Target);
        if (hits <= 0) return;

        // 性癖1つにつき1ヒット（CSV: 攻撃回数N回）
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hits)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5M);
}
