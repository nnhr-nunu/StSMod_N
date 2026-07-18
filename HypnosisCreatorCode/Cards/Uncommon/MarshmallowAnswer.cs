using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// マシュマロ回答 — ランダムな呪いカードを手札に加える。攻撃意図の敵がいれば、代わりに大きなブロックを得て沼を付与する。廃棄。
/// UGでは加える呪いが1枚に減る。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MarshmallowAnswer() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new DynamicVar("Block", 39M),
        new PowerVar<BogPower>(2M)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<BogPower>()];

    private static bool IsCurse(CardModel c) => c.Type == CardType.Curse;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        var pool = ModelDb.AllCards.Where(IsCurse).ToList();
        if (pool.Count > 0)
        {
            var rng = Owner.RunState.Rng.CombatCardSelection;
            for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
            {
                var canonical = pool[rng.NextInt(pool.Count)];
                var generated = CombatState.CreateCard(canonical, Owner);
                await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, Owner);
            }
        }

        var attacker = CombatState.HittableEnemies.FirstOrDefault(e => e.Monster?.IntendsToAttack == true);
        if (attacker == null) return;

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["Block"].BaseValue, ValueProp.Move, null);
        await PowerCmd.Apply<BogPower>(
            choiceContext, attacker, DynamicVars["BogPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(-1M);
}
