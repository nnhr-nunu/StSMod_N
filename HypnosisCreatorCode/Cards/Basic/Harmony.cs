using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>調和 — 相手の攻撃と同値のブロック。廃棄。脆弱等デバフ影響なし・連撃は合計（CSV備考）。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Harmony() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    // Unpowered: 敏捷は乗せない。プレビューは IntentDerivedPreviewPatch が意図合計で上書き。
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(0M, ValueProp.Unpowered)];

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(c => EnemyAttackIntents.GetTotalDamage(c) > 0);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var block = EnemyAttackIntents.GetTotalDamage(play.Target);
        if (block > 0)
            await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Unpowered, play);
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
