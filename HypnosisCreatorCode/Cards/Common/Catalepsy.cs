using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>
/// カタレプシー — 本家スロー（石像エリートと同種）を1付与。
/// 2枚目以降（対象に既にスローがある場合）はスローが2ずつ進む（CSV備考）。
/// UG: 対象がトランス中ならスロー蓄積がリセットされない。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Catalepsy() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Slow", 1M)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<SlowPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        // 備考: 2枚以上発動したときはスローカウントが2ずつ進む
        var alreadySlowed = play.Target.GetPowerAmount<SlowPower>() > 0;
        var slowAmount = alreadySlowed ? 2M : DynamicVars["Slow"].BaseValue;

        await PowerCmd.Apply<SlowPower>(
            choiceContext, play.Target, slowAmount, Owner.Creature, this);

        if (IsUpgraded)
        {
            await PowerCmd.Apply<CatalepsyPower>(
                choiceContext, play.Target, 1M, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() { }
}
