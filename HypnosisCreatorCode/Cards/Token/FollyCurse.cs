using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using HypnosisCreator.HypnosisCreatorCode.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Token;

/// <summary>愚行 — 天賦・エセリアル・1ドロー・廃棄・永劫。SM。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FollyCurse() : PlayableCurseCard(0,
    CardType.Curse, CardRarity.Curse, TargetType.AnyEnemy)
{
    public override string PortraitPath => "folly.png".CardImagePath();
    public override string CustomPortraitPath => "folly.png".BigCardImagePath();

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Innate, CardKeyword.Ethereal, CardKeyword.Exhaust, CardKeyword.Eternal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(1)];

    protected override async Task PlayCurseEffect(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

}
