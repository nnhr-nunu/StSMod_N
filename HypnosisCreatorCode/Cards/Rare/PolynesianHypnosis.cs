using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// ポリネシアン催眠 — カウント。相手すべてに睡眠2、沼2、トランス1。UGで睡眠3・沼3。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class PolynesianHypnosis() : HypnosisCreatorCard(6,
    CardType.Power, CardRarity.Uncommon,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => CountKeywords;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AsleepPower>(2M),
        new DynamicVar("Bog", 2M),
        new DynamicVar("Trance", 1M)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
    [
        HoverTipFactory.FromPower<AsleepPower>(),
        HoverTipFactory.FromPower<BogPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<AsleepPower>(
                choiceContext, enemy, DynamicVars["AsleepPower"].BaseValue, Owner.Creature, this);
            await PowerCmd.Apply<BogPower>(
                choiceContext, enemy, DynamicVars["Bog"].BaseValue, Owner.Creature, this);
            await TranceCombat.ApplyTrance(
                choiceContext, enemy, DynamicVars["Trance"].IntValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AsleepPower"].UpgradeValueBy(1M);
        DynamicVars["Bog"].UpgradeValueBy(1M);
    }
}
