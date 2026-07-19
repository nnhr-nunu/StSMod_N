using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>
/// 催眠導入 — 2枚ドロー（同名・同一対象プレイで減衰）。UGで基礎3枚。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class HypnosisIntro() : HypnosisCreatorCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Draw", 2M),
        new CalculationBaseVar(0M),
        new CalculationExtraVar(1M),
        new CalculatedVar("CurrentDraw").WithMultiplier(CalcCurrentDraw)
    ];

    private static decimal CalcCurrentDraw(CardModel card, Creature? target)
    {
        var baseDraw = card.DynamicVars["Draw"].BaseValue;
        if (target == null || card.Owner == null) return baseDraw;
        var prior = HypnosisIntroDrawTracker.GetPriorPlayCount(card.Owner, target, card.Id.Entry);
        return Math.Max(0, (int)baseDraw - prior);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var draw = (int)CalcCurrentDraw(this, play.Target);
        if (draw > 0)
            await CardPileCmd.Draw(choiceContext, draw, Owner);

        HypnosisIntroDrawTracker.RecordPlay(Owner, play.Target, Id.Entry);
    }

    protected override void OnUpgrade() => DynamicVars["Draw"].UpgradeValueBy(1M);
}
