using BaseLib.Patches.Localization;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
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
/// 暗示解除 — 0ダメージ（筋力あり）アタック。対象のトランスを全て解除し、
/// 解除した分だけエナジーとドローを得る（UGでエナジー2倍）。廃棄。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class SuggestionRelease() : HypnosisCreatorCard(0,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    static SuggestionRelease()
    {
        DescriptionOverrides.CustomizeDescriptionPost += AppendEffectPreview;
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(0M, ValueProp.Move)];

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(TranceCombat.HasTrance);

    private static int CalcTranceRemoval(Creature? target) =>
        target == null ? 0 : TranceCombat.GetTrance(target);

    private static int CalcEnergyGain(SuggestionRelease card, int tranceRemoved) =>
        tranceRemoved * (card.IsUpgraded ? 2 : 1);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);

        var stacks = TranceCombat.GetTrance(play.Target);
        if (stacks <= 0) return;

        var energy = CalcEnergyGain(this, stacks);

        var trance = play.Target.GetPower<TrancePower>();
        if (trance != null) await PowerCmd.Remove(trance);

        await PlayerCmd.GainEnergy(energy, Owner);
        await CardPileCmd.Draw(choiceContext, energy, Owner);
    }

    protected override void OnUpgrade() { }

    private static void AppendEffectPreview(CardModel card, Creature? target, ref string description)
    {
        if (card is not SuggestionRelease release) return;
        if (!CombatPreviewText.IsActive(release)) return;

        var previewTarget = target ?? release.CurrentTarget;
        var trance = CalcTranceRemoval(previewTarget);
        if (trance <= 0) return;

        var damage = CardDamagePreview.ApplyModifiers(
            release, previewTarget, 0M, ValueProp.Move);
        var energy = CalcEnergyGain(release, trance);

        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（ダメージ：{(int)damage}／エナジー：{energy}／ドロー：{energy}枚）"
            : $" (Damage: {(int)damage} / Energy: {energy} / Draw: {energy})";
        CombatPreviewText.AppendSuffix(release, ref description, suffix);
    }
}
