using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// ポリネシア式催眠 — 全体トランス2＋破滅8。コスト6の重量級だが[gold]カウント[/gold]を持つ。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class PolynesianHypnosis() : HypnosisCreatorCard(6,
    CardType.Skill, CardRarity.Rare,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Trance", 2M),
        new DynamicVar("Doom", 8M)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            await TranceCombat.ApplyTrance(
                choiceContext, enemy, DynamicVars["Trance"].IntValue, Owner.Creature, this);
            await FetishCombat.ApplyDoom(
                choiceContext, enemy, DynamicVars["Doom"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Trance"].UpgradeValueBy(1M);
}
