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
/// 急所の一刺し — アブノーマル。アタック未プレイのターン数が多いほどダメージ増加。
/// 合計は {CalculatedDamage:diff()} で枠・説明・緑表示を統一。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class VitalPoint() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(10M),
        new ExtraDamageVar(10M),
        new DynamicVar("PerTurn", 10M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(IdleTurnMultiplier)
    ];

    internal static decimal ComputeDamage(CardModel card)
    {
        if (card.Owner == null)
            return card.DynamicVars.CalculationBase.BaseValue;

        var turn = card.Owner.PlayerCombatState?.TurnNumber ?? 0;
        var gap = PlayerAttackTracker.TurnsSinceLastAttack(card.Owner, turn);
        return card.DynamicVars.CalculationBase.BaseValue
               + gap * card.DynamicVars.ExtraDamage.BaseValue;
    }

    private static decimal IdleTurnMultiplier(CardModel card, Creature? _)
    {
        if (card.Owner == null) return 0M;

        var turn = card.Owner.PlayerCombatState?.TurnNumber ?? 0;
        return PlayerAttackTracker.TurnsSinceLastAttack(card.Owner, turn);
    }

    protected override bool ShouldGlowWhenConditionMet()
    {
        var turn = Owner.PlayerCombatState?.TurnNumber ?? 0;
        return PlayerAttackTracker.TurnsSinceLastAttack(Owner, turn) > 0;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(5M);
        DynamicVars["PerTurn"].UpgradeValueBy(5M);
    }

    internal static void AppendDescriptionSuffix(CardModel card, Creature? target, ref string description)
    {
        if (card is not VitalPoint vital) return;

        var raw = ComputeDamage(vital);
        if (raw <= 0) return;

        CombatDamageSuffixPreview.AppendDealDamageSuffix(vital, target, ref description, raw, ValueProp.Move);
    }
}
