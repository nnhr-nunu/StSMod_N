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

/// <summary>トランスに溶けゆく — これまでに対象へ付与したトランスの合計回数×係数のダメージを与える。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MeltIntoTrance() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    static MeltIntoTrance()
    {
        DescriptionOverrides.CustomizeDescriptionPost += AppendDamagePreview;
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("PerTrance", 15M),
        new DamageVar(0M, ValueProp.Move)
    ];

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(c => TranceFallTracker.Get(c) > 0);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var fallen = TranceFallTracker.Get(play.Target);
        DynamicVars.Damage.BaseValue = fallen * DynamicVars["PerTrance"].BaseValue;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars["PerTrance"].UpgradeValueBy(5M);

    private static void AppendDamagePreview(CardModel card, Creature? target, ref string description)
    {
        if (card is not MeltIntoTrance melt) return;

        var previewTarget = target ?? melt.CurrentTarget;
        var fallen = previewTarget != null ? TranceFallTracker.Get(previewTarget) : 0;
        var per = melt.DynamicVars["PerTrance"].BaseValue;
        var total = fallen * per;

        var suffix = UpgradeCardText.IsJapaneseUi()
            ? $"（現在：{fallen}回／{total}ダメージ）"
            : $" (Now: {fallen}× / {total} damage)";

        if (description.Contains(suffix, StringComparison.Ordinal)) return;
        description = description.TrimEnd() + suffix;
    }
}
