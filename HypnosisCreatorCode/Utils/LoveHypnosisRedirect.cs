using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>好き好き催眠 — 敵のバフ／ブロック付与をプレイヤーへ付け替える判定。</summary>
public static class LoveHypnosisRedirect
{
    public static bool TryRetargetPower(Creature? applier, Creature target, out Creature newTarget)
    {
        newTarget = target;
        if (applier == null) return false;
        if (target.Side != CombatSide.Enemy) return false;

        var power = applier.GetPower<LoveHypnosisPower>();
        if (power is not { StealBuff: true }) return false;
        if (!power.IsStealingBuffMove()) return false;

        var player = power.ResolvePlayerCreature();
        if (player is not { IsAlive: true }) return false;

        newTarget = player;
        return true;
    }

    public static bool TryRetargetBlock(Creature creature, out Creature newTarget)
    {
        newTarget = creature;
        if (creature.Side != CombatSide.Enemy) return false;

        // GainBlock は applier が無いので、行動中かつ StealBlock の敵を探す
        var combat = creature.CombatState;
        if (combat == null) return false;

        foreach (var enemy in combat.HittableEnemies)
        {
            var power = enemy.GetPower<LoveHypnosisPower>();
            if (power is not { StealBlock: true }) continue;
            if (!power.IsStealingBlockMove()) continue;

            var player = power.ResolvePlayerCreature();
            if (player is not { IsAlive: true }) continue;

            newTarget = player;
            return true;
        }

        // 自分がブロックを得ようとしていて、自分にパワーがある場合
        var selfPower = creature.GetPower<LoveHypnosisPower>();
        if (selfPower is { StealBlock: true } && selfPower.IsStealingBlockMove())
        {
            var player = selfPower.ResolvePlayerCreature();
            if (player is { IsAlive: true })
            {
                newTarget = player;
                return true;
            }
        }

        return false;
    }

    public static bool HasBuffIntent(Creature enemy) =>
        enemy.Monster?.NextMove?.Intents?.OfType<BuffIntent>().Any() == true;

    public static bool HasDefendIntent(Creature enemy) =>
        enemy.Monster?.NextMove?.Intents?.OfType<DefendIntent>().Any() == true;
}
