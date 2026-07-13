using BaseLib.Extensions;
using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>快の循環 — トランス軸。Sub 付与＋Dom 獲得。一致で割合ダメ／不一致で敵回復。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class PleasureCycle() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    private const decimal MatchPercent = 10M;
    private const decimal MismatchHeal = 4M;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<SubmissionPower>(1M),
        new PowerVar<DominationPower>(1M),
        new DynamicVar("MatchPercent", MatchPercent),
        new DynamicVar("MismatchHeal", MismatchHeal)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SubmissionPower>(),
        HoverTipFactory.FromPower<DominationPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<SubmissionPower>(
            choiceContext, play.Target, DynamicVars.Power<SubmissionPower>().IntValue, Owner.Creature, this);
        await PowerCmd.Apply<DominationPower>(
            choiceContext, Owner.Creature, DynamicVars.Power<DominationPower>().IntValue, Owner.Creature, this);
        await TranceMatch.ResolveMatchOutcome(
            choiceContext, Owner.Creature, play.Target, this, MatchPercent, MismatchHeal);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Power<SubmissionPower>().UpgradeValueBy(1M);
        DynamicVars["MatchPercent"].UpgradeValueBy(5M);
    }
}
