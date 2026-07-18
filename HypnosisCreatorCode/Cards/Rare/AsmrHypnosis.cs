using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// ASMR催眠 — CSV: 複数の音源演出（複合効果）を想定。演出・個別音源の仕様未確定のため、
/// 暫定で敵全体に弱体・脆弱・トランスを付与する複合デバフとして実装。
/// TODO: 音源ごとの個別演出・効果が確定したら差し替える。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class AsmrHypnosis() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Rare,
    TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WeakPower>(1M),
        new PowerVar<FrailPower>(1M),
        new DynamicVar("Trance", 1M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<FrailPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;
        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars.Weak.BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<FrailPower>(choiceContext, enemy, DynamicVars["FrailPower"].BaseValue, Owner.Creature, this);
            await TranceCombat.ApplyTrance(choiceContext, enemy, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Trance"].UpgradeValueBy(1M);
}
