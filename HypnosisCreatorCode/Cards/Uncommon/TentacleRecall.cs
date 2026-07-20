using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>触手の想起 — 締め付け5のみ。次ターン開始時にコピーを手札へ。UGで締め付け7。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class TentacleRecall() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<ConstrictPower>(5M)];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
        [ConstrictPowerHcText.CardHoverTip()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<ConstrictPower>(
            choiceContext, play.Target, DynamicVars["ConstrictPower"].BaseValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);

        await PowerCmd.Apply<TentacleRecallPower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);
        var power = Owner.Creature.GetPower<TentacleRecallPower>();
        if (power != null) power.SourceCard = this;
    }

    protected override void OnUpgrade() => DynamicVars["ConstrictPower"].UpgradeValueBy(2M);
}
