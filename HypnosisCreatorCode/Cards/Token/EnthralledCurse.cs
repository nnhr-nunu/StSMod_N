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

/// <summary>心酔 — 必ず性癖に刺さる。廃棄。永劫。全性癖。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class EnthralledCurse() : PlayableCurseCard(0,
    CardType.Curse, CardRarity.Curse, TargetType.AnyEnemy)
{
    public override string PortraitPath => "enthralled.png".CardImagePath();
    public override string CustomPortraitPath => "enthralled.png".BigCardImagePath();

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub, FetishType.Sm, FetishType.Abnormal];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust, CardKeyword.Eternal];

    public override bool AlwaysHitsFetish => true;

    protected override async Task PlayCurseEffect(PlayerChoiceContext choiceContext, CardPlay play)
    {
    }

}
