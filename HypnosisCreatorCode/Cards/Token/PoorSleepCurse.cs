using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>寝不足 — 保留。次ターン開始時に睡眠1。廃棄。SM。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class PoorSleepCurse() : PlayableCurseCard(0,
    CardType.Curse, CardRarity.Curse, TargetType.AnyEnemy)
{
    public override string PortraitPath => "poor_sleep.png".CardImagePath();
    public override string CustomPortraitPath => "poor_sleep.png".BigCardImagePath();

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<AsleepPower>(1M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<AsleepPower>()];

    protected override async Task PlayCurseEffect(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<NextTurnAsleepPower>(
            choiceContext, play.Target, DynamicVars["AsleepPower"].BaseValue, Owner.Creature, this);
    }
}
