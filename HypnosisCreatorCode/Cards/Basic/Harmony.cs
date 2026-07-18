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
/// 調和 — 相手の攻撃と同値のブロック。
/// 脆弱等のデバフ／Dex の影響を受けない。連撃は合計。
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
        {
            // Unpowered: Dex／脆弱によるブロック増減を受けない（CSV: デバフ影響なし）
            await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Unpowered, play);
        }
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);

    /// <summary>
    /// 意図に表示される攻撃の生値（ModifyDamage前）を連撃合計する。
    /// </summary>
    internal static int CalcEnemyAttackValue(Creature enemy)
    {
        var monster = enemy.Monster;
        if (monster == null || !monster.IntendsToAttack) return 0;

        var move = monster.NextMove;
        if (move?.Intents == null) return 0;

        var total = 0;
        foreach (var intent in move.Intents)
        {
            if (intent is not AttackIntent attack) continue;
            if (attack.DamageCalc == null) continue;

            var perHit = (int)attack.DamageCalc();
            var repeats = Math.Max(1, attack.Repeats);
            total += Math.Max(0, perHit) * repeats;
        }

        return Math.Max(0, total);
    }
}
