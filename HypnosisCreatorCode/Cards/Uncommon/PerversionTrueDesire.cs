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

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>性的倒錯の本懐 — アブノーマル。自身の欠損HP分のダメージを与える。廃棄（UGで廃棄しない）。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class PerversionTrueDesire() : HypnosisCreatorCard(2,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    static PerversionTrueDesire()
    {
        DescriptionOverrides.CustomizeDescriptionPost += AppendDamagePreview;
    }
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0M),
        new ExtraDamageVar(1M),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(CalcMissingHp)
    ];

    internal static decimal ComputeDamage(CardModel card) => CalcMissingHp(card, null);

    private static decimal CalcMissingHp(CardModel card, Creature? target)
    {
        _ = target;
        var self = card.Owner?.Creature;
        if (self == null) return 0M;
        return Math.Max(0, self.MaxHp - self.CurrentHp);
    }

    protected override bool ShouldGlowWhenConditionMet()
    {
        var self = Owner.Creature;
        return self != null && self.CurrentHp < self.MaxHp;
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

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);

    private static void AppendDamagePreview(CardModel card, Creature? target, ref string description)
    {
        if (card is not PerversionTrueDesire desire) return;

        var raw = ComputeDamage(desire);
        if (raw <= 0) return;

        CombatDamageSuffixPreview.AppendDealDamageSuffix(desire, target, ref description, raw, ValueProp.Move);
    }
}
