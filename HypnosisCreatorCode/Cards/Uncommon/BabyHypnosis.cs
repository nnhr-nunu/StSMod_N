using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 赤ちゃん催眠 — カウント。対象のバフをすべて解除し、縮小2・弱体2・トランス1。
/// UG: 縮小4・弱体4。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class BabyHypnosis() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ShrinkPower>(2M),
        new PowerVar<WeakPower>(2M),
        new DynamicVar("Trance", 1M)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
    [
        HoverTipFactory.FromPower<ShrinkPower>(),
        HoverTipFactory.FromPower<WeakPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        foreach (var power in play.Target.Powers.Where(p => p.Type == PowerType.Buff).ToList())
            await PowerCmd.Remove(power);

        await PowerCmd.Apply<ShrinkPower>(
            choiceContext, play.Target, DynamicVars["ShrinkPower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(
            choiceContext, play.Target, DynamicVars["WeakPower"].BaseValue, Owner.Creature, this);
        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ShrinkPower"].UpgradeValueBy(2M);
        DynamicVars["WeakPower"].UpgradeValueBy(2M);
    }
}
