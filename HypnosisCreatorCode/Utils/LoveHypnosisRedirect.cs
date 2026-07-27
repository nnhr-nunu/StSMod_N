using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>好き好き催眠 — 敵のバフ／ブロック付与をプレイヤーへ付け替える判定。</summary>
public static class LoveHypnosisRedirect
{
    public static bool TryRetargetPower(Creature? applier, Creature target, out Creature newTarget)
    {
        return TryRetargetPower(applier, target, null, out newTarget);
    }

  public static bool TryRetargetPower(Creature? applier, Creature target, PowerModel? appliedPower, out Creature newTarget)
    {
        newTarget = target;
        if (applier == null) return false;
        if (target.Side != CombatSide.Enemy) return false;
        if (appliedPower != null && appliedPower.Type != PowerType.Buff) return false;

        if (!TryGetActiveLoveHypnosis(applier, out var hypnosis) || !hypnosis.StealBuff)
            return false;

        var player = hypnosis.ResolvePlayerCreature();
        if (player is not { IsAlive: true }) return false;

        newTarget = player;
        return true;
    }

    public static bool TryRetargetBlock(Creature creature, out Creature newTarget)
    {
        newTarget = creature;
        if (creature.Side != CombatSide.Enemy) return false;

        var combat = creature.CombatState;
        if (combat == null) return false;

        foreach (var enemy in combat.HittableEnemies)
        {
            if (!TryGetActiveLoveHypnosis(enemy, out var hypnosis) || !hypnosis.StealBlock)
                continue;

            var player = hypnosis.ResolvePlayerCreature();
            if (player is not { IsAlive: true }) continue;

            newTarget = player;
            return true;
        }

        if (TryGetActiveLoveHypnosis(creature, out var selfHypnosis) && selfHypnosis.StealBlock)
        {
            var player = selfHypnosis.ResolvePlayerCreature();
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

    private static bool TryGetActiveLoveHypnosis(Creature applier, out LoveHypnosisPower hypnosis)
    {
        hypnosis = applier.GetPower<LoveHypnosisPower>()!;
        return hypnosis != null;
    }
}
