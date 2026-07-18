using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 完全掌握 — CSV: 本来は「対象の攻撃意図をこちらへ向けさせる（攻撃対象リダイレクト）」効果。
/// リダイレクトAPIが未確定のため、暫定で重い弱体化（弱体2・脆弱2・筋力減2）で近似実装。
/// TODO: sts2 側にターゲットリダイレクトAPIが見つかったら本実装に置き換える。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class TotalControl() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(2M),
        new PowerVar<FrailPower>(2M),
        new PowerVar<StrengthPower>("StrengthLoss", 2M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<FrailPower>(),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<WeakPower>(
            choiceContext, play.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<FrailPower>(
            choiceContext, play.Target, DynamicVars["FrailPower"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, play.Target, -DynamicVars["StrengthLoss"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["StrengthLoss"].UpgradeValueBy(1M);
}
