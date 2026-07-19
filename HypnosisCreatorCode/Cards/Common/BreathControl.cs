using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>呼吸制御 — SM。このターン、カードプレイごとに対象の筋力−1。攻撃0ならスタン。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class BreathControl() : HypnosisCreatorCard(0,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Sm];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [HoverTipFactory.FromPower<BreathControlPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<BreathControlPower>(
            choiceContext, play.Target, 1M, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
