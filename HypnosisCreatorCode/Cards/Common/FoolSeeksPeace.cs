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

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>愚者は平和… — トランス軸。スタン＋一致判定。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FoolSeeksPeace() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    private const decimal MatchPercent = 12M;
    private const decimal MismatchHeal = 5M;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<SubmissionPower>(2M),
        new DynamicVar("MatchPercent", MatchPercent),
        new DynamicVar("MismatchHeal", MismatchHeal)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<SubmissionPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<SubmissionPower>(
            choiceContext, play.Target, DynamicVars.Power<SubmissionPower>().IntValue, Owner.Creature, this);
        await CreatureCmd.Stun(play.Target);
        await TranceMatch.ResolveMatchOutcome(
            choiceContext, Owner.Creature, play.Target, this, MatchPercent, MismatchHeal);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars["MatchPercent"].UpgradeValueBy(3M);
    }
}
