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

/// <summary>
/// ゼロへの近道 — 3→2→1→0ブロック（合計6）を得て、手札カウントコストを0にする。UGで2コスト。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class ZeroShortcut() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    static ZeroShortcut()
    {
        DescriptionOverrides.CustomizeDescriptionPost += AppendTotalBlockPreview;
    }

    private const int StartBlock = 3;

    public override bool GainsBlock => true;

    protected override bool ShouldGlowWhenConditionMet()
    {
        var hand = Owner.PlayerCombatState?.Hand;
        return hand != null && hand.Cards.Any(c =>
            CountRules.HasCountKeyword(c) && c.EnergyCost.GetResolved() > 0);
    }

    // 3+2+1+0 = 6。説明の合計ブロックは {Block:diff()} でプレビューする。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(StartBlock * (StartBlock + 1) / 2, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        for (var block = StartBlock; block >= 0; block--)
            await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, play);

        CountRules.ZeroHandCountCosts(Owner);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);

    private static void AppendTotalBlockPreview(CardModel card, Creature? _, ref string description)
    {
        if (card is not ZeroShortcut shortcut) return;

        var total = shortcut.DynamicVars.Block.BaseValue;
        CombatDamageSuffixPreview.AppendBlockGainSuffix(shortcut, ref description, total, total);
    }
}
