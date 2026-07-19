using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// ゼロへの近道 — 3→2→1→0ブロックを得て、手札カウントコストを0にする。UGで2コスト。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class ZeroShortcut() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override bool ShouldGlowWhenConditionMet()
    {
        var hand = Owner.PlayerCombatState?.Hand;
        return hand != null && hand.Cards.Any(c =>
            CountRules.HasCountKeyword(c) && c.EnergyCost.GetResolved() > 0);
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(6M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        for (var block = 3; block >= 0; block--)
            await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, play);

        CountRules.ZeroHandCountCosts(Owner);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
