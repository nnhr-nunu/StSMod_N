using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 完全掌握 — DomSub。コスト1。対象のトランス≥2が必要。
/// このターン、プレイヤーへの攻撃ダメージを対象が肩代わり。廃棄（UGで消滅）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class TotalControl() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    internal const int MinTranceRequired = 2;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub];

    // トランス≥2 がいないとプレイ不可 → 性癖一致だけでは光らせない
    protected override bool FetishGlowAllowed => ShouldGlowWhenConditionMet();

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfTargetOrAnyEnemy(c => TranceCombat.GetTrance(c) >= MinTranceRequired);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (TranceCombat.GetTrance(play.Target) < MinTranceRequired) return;

        await PowerCmd.Apply<TotalControlPower>(
            choiceContext, play.Target, 1M, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);

    internal static bool IsValidTranceTarget(Creature target) =>
        target.IsAlive && TranceCombat.GetTrance(target) >= MinTranceRequired;

    internal static bool CanStartPlay(CardModel card) =>
        card.CombatState?.HittableEnemies.Any(e => e.IsAlive && e.IsEnemy && IsValidTranceTarget(e)) ?? false;
}
