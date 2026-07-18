using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>感度3000倍 — トランス3＋このターン被ダメージ×3。性癖3種必ず刺さる。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Sensitivity3000() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    public override IReadOnlyList<FetishType> CardFetishes =>
        [FetishType.Sm, FetishType.DomSub, FetishType.Abnormal];

    public override bool AlwaysHitsFetish => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Trance", 3M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        await PowerCmd.Apply<SensitivityPower>(
            choiceContext, play.Target, 1M, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => DynamicVars["Trance"].UpgradeValueBy(1M);
}
