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
/// お仕置き — 対象がプレイヤーを攻撃した回数×8ダメージ（UG13）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Punishment() : HypnosisCreatorCard(2,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(c => EnemyPlayerAttackTracker.GetCount(c) > 0);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8M, ValueProp.Move),
        new CalculationBaseVar(0M),
        new CalculationExtraVar(1M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalcTotalDamage),
        new CalculatedVar("HitCount").WithMultiplier(CalcHitCount)
    ];

    private static decimal CalcHitCount(CardModel card, Creature? target) =>
        target == null ? 0M : EnemyPlayerAttackTracker.GetCount(target);

    private static decimal CalcTotalDamage(CardModel card, Creature? target) =>
        card.DynamicVars.Damage.BaseValue * CalcHitCount(card, target);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        if (CalcTotalDamage(this, play.Target) > 0)
        {
            await DamageCmd.Attack(DynamicVars.CalculatedDamage)
                .FromCard(this, play)
                .Targeting(play.Target)
                .WithHitFx("vfx/vfx_attack_blunt", tmpSfx: "blunt_attack.mp3")
                .Execute(choiceContext);
        }

        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(5M);
}
