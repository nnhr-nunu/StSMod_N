using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// オールインワン — CSV: 弱体・脆弱・トランス・破滅を一括で盛り込む複合カード。
/// 個別数値の最終調整は未確定のため、既存デバフ効果を合算した暫定値で実装。
/// TODO: CSVの正式な数値が確定したら調整する。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AllInOne() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(1M),
        new PowerVar<FrailPower>(1M),
        new DynamicVar("Trance", 1M),
        new DynamicVar("Doom", 4M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<FrailPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<WeakPower>(
            choiceContext, play.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<FrailPower>(
            choiceContext, play.Target, DynamicVars["FrailPower"].BaseValue, Owner.Creature, this);
        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await FetishCombat.ApplyDoom(
            choiceContext, play.Target, DynamicVars["Doom"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Doom"].UpgradeValueBy(4M);
}
