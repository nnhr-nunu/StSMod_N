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
/// カタレプシー — 本家スロー（石像エリートと同種）を1付与。重複適用でその分カウントが進む。
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
        await PowerCmd.Apply<SlowPower>(
            choiceContext, play.Target, DynamicVars["Slow"].BaseValue, Owner.Creature, this);

        if (IsUpgraded)
        {
            await PowerCmd.Apply<CatalepsyPower>(
                choiceContext, play.Target, 1M, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() { }
}
