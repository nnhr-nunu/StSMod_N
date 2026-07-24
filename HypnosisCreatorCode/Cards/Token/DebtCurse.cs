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

/// <summary>負債 — 戦闘終了後に追加10ゴールド。廃棄。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class DebtCurse() : PlayableCurseCard(0,
    CardType.Curse, CardRarity.Curse, TargetType.AnyEnemy)
{
    public override string PortraitPath => "debt.png".CardImagePath();
    public override string CustomPortraitPath => "debt.png".BigCardImagePath();

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new GoldVar(10)];

    protected override Task PlayCurseEffect(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ProselytizeRewards.AddGold(Owner, DynamicVars.Gold.BaseValue);
        return Task.CompletedTask;
    }

}
