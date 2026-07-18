using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>調和 — 相手の攻撃と同値のブロック。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Harmony() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Basic,
    TargetType.AnyEnemy)
{
    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var block = CalcEnemyAttackValue(play.Target);
        if (block > 0)
            await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, play);
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);

    private static int CalcEnemyAttackValue(MegaCrit.Sts2.Core.Entities.Creatures.Creature enemy)
    {
        var monster = enemy.Monster;
        if (monster == null || !monster.IntendsToAttack) return 0;

        var move = monster.NextMove;
        if (move?.Intents == null) return 0;

        var total = 0;
        foreach (var intent in move.Intents)
        {
            if (intent is AttackIntent attack)
                total += (int)attack.DamageCalc() * Math.Max(1, attack.Repeats);
        }

        return Math.Max(0, total);
    }
}
