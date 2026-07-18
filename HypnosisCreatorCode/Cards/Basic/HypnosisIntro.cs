using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

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
        [new DynamicVar("Draw", 2M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        var cardKey = Id.Entry;
        var prior = HypnosisIntroDrawTracker.GetPriorPlayCount(Owner, play.Target, cardKey);
        var draw = Math.Max(0, DynamicVars["Draw"].IntValue - prior);

        if (draw > 0)
            await CardPileCmd.Draw(choiceContext, draw, Owner);

        HypnosisIntroDrawTracker.RecordPlay(Owner, play.Target, cardKey);
    }

    protected override void OnUpgrade() => DynamicVars["Draw"].UpgradeValueBy(1M);
}
