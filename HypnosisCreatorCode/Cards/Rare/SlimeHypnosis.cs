using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// スライム催眠 — カウント・アブノーマル。1ターン意図を粘液×5（UG×8）へ上書き＋トランス1。
/// 見た目差し替えは任意スタブ。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class SlimeHypnosis() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Slimed", 5M),
        new DynamicVar("Trance", 1M)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<SlimeHypnosisPower>(
            choiceContext, play.Target, DynamicVars["Slimed"].BaseValue, Owner.Creature, this);
        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    // UG: 粘液 5→8（CSVの強化方向に合わせる。表記 "UG x3" は +3 と解釈）
    protected override void OnUpgrade() => DynamicVars["Slimed"].UpgradeValueBy(3M);
}
