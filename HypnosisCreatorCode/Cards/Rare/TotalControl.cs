using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 完全掌握 — DomSub。コスト1。対象のトランス≥3が必要。
/// このターン攻撃ダメージを自分へ肩代わり。廃棄（UGで消滅）。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class TotalControl() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        if (TranceCombat.GetTrance(play.Target) < 3) return;

        await PowerCmd.Apply<TotalControlPower>(
            choiceContext, Owner.Creature, 1M, Owner.Creature, this);
        await ResolveFetishOnTarget(choiceContext, play);
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
