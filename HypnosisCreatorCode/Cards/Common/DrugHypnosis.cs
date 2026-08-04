using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>薬物催眠 — カウント。毒7、破滅7、弱体2、脱力2、筋力低下1、沼1、トランス1。UGで10/10/3/3/2/2/1。アブノーマル性癖。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class DrugHypnosis() : HypnosisCreatorCard(3,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.Abnormal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<PoisonPower>(7M),
        new DynamicVar("Doom", 7M),
        new PowerVar<VulnerablePower>(2M),
        new PowerVar<WeakPower>(2M),
        new PowerVar<StrengthPower>("StrengthLoss", 1M),
        new DynamicVar("Bog", 1M),
        new DynamicVar("Trance", 1M)
    ];

    // 効果は説明文の [gold] キーワード＋MechanicKeywordPatch（トランス／破滅／沼）で足す。
    // 本家デバフ5種の CardHoverTips は過多になり戦闘ホバーが見切れるため付けない。

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);

        await PowerCmd.Apply<PoisonPower>(
            choiceContext, play.Target, DynamicVars.Poison.BaseValue, Owner.Creature, this);
        await FetishCombat.ApplyDoom(
            choiceContext, play.Target, DynamicVars["Doom"].IntValue, Owner.Creature, this);
        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, play.Target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(
            choiceContext, play.Target, DynamicVars.Weak.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, play.Target, -DynamicVars["StrengthLoss"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<BogPower>(
            choiceContext, play.Target, DynamicVars["Bog"].BaseValue, Owner.Creature, this);
        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Poison.UpgradeValueBy(3M);
        DynamicVars["Doom"].UpgradeValueBy(3M);
        DynamicVars["StrengthLoss"].UpgradeValueBy(1M);
        DynamicVars.Vulnerable.UpgradeValueBy(1M);
        DynamicVars.Weak.UpgradeValueBy(1M);
        DynamicVars["Bog"].UpgradeValueBy(1M);
        // トランスは UG 後も 1 のまま
    }
}
