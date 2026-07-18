using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 植物寄生催眠 — カウント・ハート・アブノーマル。ダメージ、収縮、トランスを付与する。
/// CSV: 撃破時の心臓入手効果は mechanics-lock.md「個別心臓レリック効果」(後送)のため未実装。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class PlantParasiteHypnosis() : HypnosisCreatorCard(2,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15M, ValueProp.Move),
        new PowerVar<ConstrictPower>(10M),
        new DynamicVar("Trance", 1M)
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
        await TranceCombat.ApplyTrance(choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["ConstrictPower"].UpgradeValueBy(5M);
}
