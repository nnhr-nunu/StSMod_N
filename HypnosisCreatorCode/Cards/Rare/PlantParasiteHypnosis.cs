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

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 植物寄生催眠 — カウント・ハート・アブノーマル。収縮10（UG15）＋トランス。
/// 戦闘終了時、付与していれば心臓入手（キル不要）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class PlantParasiteHypnosis() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ConstrictPower>(10M),
        new DynamicVar("Trance", 1M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<ConstrictPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

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
