using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>エリクソン的誘導 — 対象の性癖を刺したとき手札カウントが1進む。UGで0コスト。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Ericksonian() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<EricksonianPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<EricksonianPower>(choiceContext, play.Target, 1M, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
