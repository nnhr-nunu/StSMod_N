using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>前戯 — 弱体1＋脱力1＋エナジー1、廃棄。UGで弱体2＋脱力2。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Prefinger() : HypnosisCreatorCard(0,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(1M),
        new PowerVar<WeakPower>(1M),
        new EnergyVar(1)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        EnergyHoverTip
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, play.Target, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(
            choiceContext, play.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VulnerablePower"].UpgradeValueBy(1M);
        DynamicVars.Weak.UpgradeValueBy(1M);
    }
}
