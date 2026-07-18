using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>ぜーろっ — カウント。筋力を一時的に0にし攻撃意図を無力化＋トランス1。UGでブロックも除去。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class ZeroOut() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Trance", 1M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        if (IsUpgraded && play.Target.Block > 0)
            await CreatureCmd.LoseBlock(choiceContext, play.Target, play.Target.Block, Owner.Creature);

        var strength = play.Target.GetPowerAmount<StrengthPower>();
        if (strength > 0)
        {
            await PowerCmd.Apply<ZeroOutPower>(
                choiceContext, play.Target, strength, Owner.Creature, this);
        }

        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
