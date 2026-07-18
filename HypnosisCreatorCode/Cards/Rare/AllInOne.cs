using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// オールインワン — カウント5。山札・手札・捨札（UGで廃棄山も）のカウント催眠を対象へ一括プレイ。
/// 自身は再プレイしない。リストはスナップショット。トランス1。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AllInOne() : HypnosisCreatorCard(5,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Trance", 1M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var combat = Owner.PlayerCombatState;
        if (combat == null) return;

        var piles = new List<CardPile> { combat.DrawPile, combat.Hand, combat.DiscardPile };
        if (IsUpgraded)
            piles.Add(combat.ExhaustPile);

        var snapshot = piles
            .SelectMany(p => p.Cards)
            .Where(c => c != this && CountRules.HasCountKeyword(c))
            .Distinct()
            .ToList();

        foreach (var card in snapshot)
            await CardCmd.AutoPlay(choiceContext, card, play.Target);

        await TranceCombat.ApplyTrance(
            choiceContext, play.Target, DynamicVars["Trance"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { }
}
