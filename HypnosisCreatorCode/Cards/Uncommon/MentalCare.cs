using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>メンタルケア — 自身の弱体・脆弱を取り除き、ブロック4を得る。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MentalCare() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(4M, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var self = Owner.Creature;

        var weak = self.Powers.OfType<WeakPower>().FirstOrDefault();
        if (weak != null) await PowerCmd.Remove(weak);

        var frail = self.Powers.OfType<FrailPower>().FirstOrDefault();
        if (frail != null) await PowerCmd.Remove(frail);

        await CreatureCmd.GainBlock(self, DynamicVars.Block, play);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3M);
}
