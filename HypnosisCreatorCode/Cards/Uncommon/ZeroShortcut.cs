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
/// ゼロへの近道 — 3→2→1→0ブロック（合計6）を得て、手札カウントコストを0にする。UGで2コスト。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class ZeroShortcut() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    internal const int StartBlock = 3;

    internal static int BaselineTotalBlock => StartBlock * (StartBlock + 1) / 2;

    internal static decimal ComputePreviewTotalBlock(
        ZeroShortcut card,
        CardPreviewMode previewMode = CardPreviewMode.Normal,
        bool runGlobalHooks = true) =>
        CardBlockPreview.SumSequentialGains(card, StartBlock, ValueProp.Move, previewMode, runGlobalHooks);

    public override bool GainsBlock => true;

    protected override bool ShouldGlowWhenConditionMet()
    {
        var hand = Owner.PlayerCombatState?.Hand;
        return hand != null && hand.Cards.Any(c =>
            CountRules.HasCountKeyword(c) && c.EnergyCost.GetWithModifiers(CostModifiers.Local) > 0);
    }

    // Block＝合計6の :diff()。FirstBlock＝初回3の敏捷・エンチャント反映。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("FirstBlock", StartBlock),
        new BlockVar(BaselineTotalBlock, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        for (var block = StartBlock; block >= 0; block--)
            await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, play);

        CountRules.ZeroHandCountCosts(Owner);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);

    internal static void AppendDescriptionSuffix(CardModel card, Creature? _, ref string description)
    {
        if (card is not ZeroShortcut shortcut) return;

        var baseline = BaselineTotalBlock;
        var total = ComputePreviewTotalBlock(shortcut);
        CombatDamageSuffixPreview.AppendBlockGainSuffix(shortcut, ref description, total, baseline);
    }
}
