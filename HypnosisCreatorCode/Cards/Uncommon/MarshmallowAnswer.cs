using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>
/// マシュマロ回答 — ランダムな呪いを手札に加える。攻撃予定の敵がいれば、その行動を
/// 「ブロック39を得る」に上書きし沼2を付与する（元の行動は消費、次ターンは別行動）。廃棄。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MarshmallowAnswer() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override bool ShouldGlowWhenConditionMet() =>
        GlowIfAnyEnemy(EnemyAttackIntents.IntendsToAttack);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new DynamicVar("Block", 39M),
        new PowerVar<BogPower>(2M)
    ];

    protected override IEnumerable<IHoverTip> CardHoverTips =>
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

        var block = DynamicVars["Block"].BaseValue;
        TryOverwriteToDefend(attacker, block);
        await PowerCmd.Apply<BogPower>(
            choiceContext, attacker, DynamicVars["BogPower"].BaseValue, Owner.Creature, this);
    }

    private static void TryOverwriteToDefend(Creature enemy, decimal block)
    {
        if (enemy.Monster == null) return;
        try
        {
            async Task OnPerform(IReadOnlyList<Creature> _)
            {
                if (!enemy.IsAlive) return;
                await CreatureCmd.GainBlock(enemy, block, ValueProp.Move, null);
            }

            var move = new MoveState(
                "hypnosis_creator_marshmallow_defend",
                OnPerform,
                [new DefendIntent()]);
            enemy.Monster.SetMoveImmediate(move, forceTransition: true);
        }
        catch
        {
            // Intent API 差異時は沼付与のみ
        }
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(-1M);
}
