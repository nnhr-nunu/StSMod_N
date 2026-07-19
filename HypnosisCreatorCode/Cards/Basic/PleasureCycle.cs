using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>快の循環 — 破滅10（UGで15）。トランス中なら追加で同量の破滅。プレイ後は手札に戻る。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class PleasureCycle() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    // CSV上はコモン。トランスタグ相当の効果
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Doom", 10M)];

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(TranceCombat.HasTrance);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var amount = DynamicVars["Doom"].IntValue;
        await FetishCombat.ApplyDoom(choiceContext, play.Target, amount, Owner.Creature, this);
        if (TranceCombat.HasTrance(play.Target))
            await FetishCombat.ApplyDoom(choiceContext, play.Target, amount, Owner.Creature, this);
    }

    protected override CardLocation GetResultLocationForCardPlay() =>
        new(Owner, PileType.Hand, CardPilePosition.Top);

    protected override void OnUpgrade() => DynamicVars["Doom"].UpgradeValueBy(5M);
}
