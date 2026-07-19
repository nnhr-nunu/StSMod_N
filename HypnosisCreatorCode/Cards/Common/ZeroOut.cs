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

/// <summary>ぜーろっ — カウント。攻撃値を0にする＋トランス1。UGでブロックも除去。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class ZeroOut() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Trance", 1M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
    [
        HoverTipFactory.FromPower<ZeroOutPower>(),
        HoverTipFactory.FromPower<TrancePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        if (IsUpgraded && play.Target.Block > 0)
            await CreatureCmd.LoseBlock(choiceContext, play.Target, play.Target.Block, Owner.Creature);

        await PowerCmd.Apply<ZeroOutPower>(
            choiceContext, play.Target, 1m, Owner.Creature, this);

        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
    }

    // UG: 攻撃値に加えブロックも0にする（OnPlay の IsUpgraded 分岐）
    protected override void OnUpgrade() { }
}
