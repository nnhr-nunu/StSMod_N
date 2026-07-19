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

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 植物寄生催眠 — カウント。15ダメージ＋収縮10（UG15）＋トランス1。
/// 「心臓寄生催眠」デバフを付与し、戦闘終了時に敵固有心臓を入手（キル不要）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class PlantParasiteHypnosis() : HypnosisCreatorCard(3,
    CardType.Attack, CardRarity.Uncommon,
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

    protected override IEnumerable<IHoverTip> CardHoverTips =>
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
        await PowerCmd.Apply<PlantParasiteMarkPower>(
            choiceContext, play.Target, 1M, Owner.Creature, this);
        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["ConstrictPower"].UpgradeValueBy(5M);
}
