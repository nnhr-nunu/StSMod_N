using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// メンタルケア — 破滅以外の対象デバフを解除し、種類×8ブロック＋沼1。UGで10/種＋沼2。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MentalCare() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(c =>
            c.Powers.Any(p => p.Type == PowerType.Debuff && p is not DoomPower));

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BlockPerType", 8M),
        new PowerVar<BogPower>(1M)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<BogPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var debuffs = play.Target.Powers
            .Where(p => p.Type == PowerType.Debuff && p is not DoomPower)
            .ToList();
        var typeCount = debuffs.Select(p => p.GetType()).Distinct().Count();

        foreach (var power in debuffs)
            await PowerCmd.Remove(power);

        if (typeCount > 0)
        {
            var block = DynamicVars["BlockPerType"].BaseValue * typeCount;
            await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, play);
        }

        await PowerCmd.Apply<BogPower>(
            choiceContext, play.Target, DynamicVars["BogPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockPerType"].UpgradeValueBy(2M);
        DynamicVars["BogPower"].UpgradeValueBy(1M);
    }
}
