using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Basic;

/// <summary>
/// 調和 — 相手の攻撃意図に表示されるダメージと同値のブロック。
/// 連撃は合計。ブロック取得時は Dex／脆弱の影響を受けない。
/// </summary>
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
            await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Unpowered, play);
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);

    /// <summary>
    /// 意図UIと同じ <see cref="AttackIntent.GetTotalDamage"/> を使う（筋力込み・連撃合計）。
    /// </summary>
    internal static int CalcEnemyAttackValue(Creature enemy)
    {
        var monster = enemy.Monster;
        if (monster == null || !monster.IntendsToAttack) return 0;

        var move = monster.NextMove;
        if (move?.Intents == null) return 0;

        var intents = move.Intents;
        var total = 0;
        foreach (var intent in intents)
        {
            if (intent is not AttackIntent attack) continue;
            total += Math.Max(0, attack.GetTotalDamage(intents, enemy));
        }

        return Math.Max(0, total);
    }
}
