using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>脳くちゅ催眠 — 攻撃リダイレクト後のデバフ付与先を、着弾した相手に合わせる。</summary>
public static class BrainSlimeRedirectRules
{
    public static bool TryRetarget(Creature? applier, ref Creature target)
    {
        if (applier == null || !target.IsPlayer) return false;
        if (applier.GetPower<BrainSlimeRedirectPower>() is not { } redirect) return false;
        if (redirect.RedirectAllEnemies) return false;

        var newTarget = redirect.GetAttackDebuffRedirectTarget();
        if (newTarget is not { IsAlive: true } || newTarget.IsPlayer) return false;

        target = newTarget;
        return true;
    }

    public static bool TryRetargetAll(Creature? applier, ref IEnumerable<Creature> targets)
    {
        if (applier == null) return false;

        var materialized = targets as IReadOnlyList<Creature> ?? targets.ToList();
        List<Creature>? rewritten = null;

        for (var i = 0; i < materialized.Count; i++)
        {
            var candidate = materialized[i];
            if (!candidate.IsPlayer) continue;
            if (!TryRetarget(applier, ref candidate)) continue;

            rewritten ??= materialized.ToList();
            rewritten[i] = candidate;
        }

        if (rewritten == null) return false;
        targets = rewritten;
        return true;
    }
}
