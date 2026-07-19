using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// 好き好き催眠 — カウント。バフ行動の対象をプレイヤーへ変更。トランス1。
/// UG: ブロック行動も奪う。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class LoveHypnosis() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Trance", 1M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<LoveHypnosisPower>()];

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(HasStealableIntent);

    private bool HasStealableIntent(Creature enemy)
    {
        if (LoveHypnosisRedirect.HasBuffIntent(enemy)) return true;
        return IsUpgraded && LoveHypnosisRedirect.HasDefendIntent(enemy);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<LoveHypnosisPower>(
            choiceContext, play.Target, 1M, Owner.Creature, this);
        var power = play.Target.GetPower<LoveHypnosisPower>();
        if (power != null)
        {
            power.StealBuff = true;
            power.StealBlock = IsUpgraded;
        }

        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() { }
}
