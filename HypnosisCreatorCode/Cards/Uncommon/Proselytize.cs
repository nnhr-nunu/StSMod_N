using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>布教欲求 — 対象に推し活（デバフ）と沼を付与。戦闘終了時にゴールド。廃棄。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Proselytize() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<OshiActivityPower>(1M),
        new PowerVar<BogPower>(2M),
        new DynamicVar("Gold", 15M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<OshiActivityPower>(),
        HoverTipFactory.FromPower<BogPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<OshiActivityPower>(
            choiceContext, play.Target, DynamicVars["OshiActivityPower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<BogPower>(
            choiceContext, play.Target, DynamicVars["BogPower"].BaseValue, Owner.Creature, this);

        ProselytizeRewards.AddGold(Owner.Creature, DynamicVars["Gold"].BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars["Gold"].UpgradeValueBy(10M);
}
