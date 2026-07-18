using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// このターン、カードがプレイされるたび所有者（敵）の筋力を1失わせる。
/// 攻撃意図の値が0ならスタン。
/// </summary>
public class BreathControlPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;

        await PowerCmd.Apply<StrengthPower>(
            choiceContext, Owner, -1m, Applier ?? Owner, cardPlay.Card);

        if (IsAttackValueZero(Owner))
            await CreatureCmd.Stun(Owner);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (side != CombatSide.Player) return;
        await PowerCmd.Remove(this);
    }

    private static bool IsAttackValueZero(Creature enemy)
    {
        var monster = enemy.Monster;
        if (monster == null || !monster.IntendsToAttack) return false;

        var move = monster.NextMove;
        if (move?.Intents == null) return false;

        var total = 0;
        foreach (var intent in move.Intents)
        {
            if (intent is not AttackIntent attack) continue;
            total += (int)attack.DamageCalc() * Math.Max(1, attack.Repeats);
        }

        return total <= 0;
    }
}
