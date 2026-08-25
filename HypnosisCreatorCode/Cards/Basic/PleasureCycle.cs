using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>
/// 快の循環 — 必ず性癖に刺さる。通常はトランス中なら8ダメージ。UGはトランス1＋8ダメージ。プレイ後は手札に戻る。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class PleasureCycle() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes =>
        [FetishType.Abnormal, FetishType.Sm, FetishType.DomSub];

    public override bool AlwaysHitsFetish => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8M, ValueProp.Move),
        new DynamicVar("Trance", 1M)
    ];

    protected override bool ShouldGlowWhenConditionMet() =>
        IsUpgraded || GlowIfTargetOrAnyEnemy(TranceCombat.HasTrance);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        if (IsUpgraded)
        {
            await TranceCombat.ApplyTrance(
                choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
            await DealDamage(choiceContext, play);
        }
        else if (TranceCombat.HasTrance(play.Target))
        {
            await DealDamage(choiceContext, play);
        }

        await ResolveFetishOnTarget(choiceContext, play);
    }

    private async Task DealDamage(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, play)
            .Targeting(play.Target!)
            .WithHitFx("vfx/vfx_attack_slash", tmpSfx: "attack_sword.mp3")
            .Execute(choiceContext);
    }

    protected override CardLocation GetResultLocationForCardPlay() =>
        new(Owner, PileType.Hand, CardPilePosition.Top);
}
