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
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>触手の想起 — アブノーマル。ダメージと収縮を与え、次の自ターン開始時にこのカードのコピーを手札に加える。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class TentacleRecall() : HypnosisCreatorCard(1,
    CardType.Attack, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9M, ValueProp.Move),
        new PowerVar<ConstrictPower>(5M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ConstrictPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);

        await PowerCmd.Apply<ConstrictPower>(
            choiceContext, play.Target, DynamicVars["ConstrictPower"].BaseValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);

        await PowerCmd.Apply<TentacleRecallPower>(choiceContext, Owner.Creature, 1M, Owner.Creature, this);
        var power = Owner.Creature.GetPower<TentacleRecallPower>();
        if (power != null) power.SourceCard = this;
    }

    protected override void OnUpgrade() => DynamicVars["ConstrictPower"].UpgradeValueBy(2M);
}
