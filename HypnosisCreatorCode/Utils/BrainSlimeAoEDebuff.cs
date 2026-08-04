using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>脳くちゅ催眠 UG — 攻撃付随デバフを相手全員へ広げる。</summary>
public struct BrainSlimeAoEDebuffState
{
    public bool Active;
    public PowerModel? Power;
    public decimal Amount;
    public Creature? Applier;
    public CardModel? CardSource;
    public List<Creature> RemainingTargets;
}

public static class BrainSlimeAoEDebuff
{
    public static bool TryBegin(Creature? applier, ref Creature target, out BrainSlimeAoEDebuffState state)
    {
        state = default;
        if (applier?.GetPower<BrainSlimeRedirectPower>() is not { RedirectAllEnemies: true } redirect)
            return false;
        if (!target.IsPlayer) return false;

        var enemies = redirect.CombatState?.HittableEnemies
            .Where(e => e is { IsAlive: true, IsEnemy: true })
            .ToList() ?? [];
        if (enemies.Count == 0) return false;

        target = enemies[0];
        state = new BrainSlimeAoEDebuffState
        {
            Active = true,
            Applier = applier,
            RemainingTargets = enemies.Skip(1).ToList()
        };
        return true;
    }

    public static async Task ContinueAfterApply(
        Task original,
        BrainSlimeAoEDebuffState state,
        PowerModel power,
        decimal amount,
        CardModel? cardSource,
        PlayerChoiceContext choiceContext)
    {
        await original;
        if (!state.Active || state.RemainingTargets.Count == 0) return;

        foreach (var enemy in state.RemainingTargets.Where(e => e.IsAlive))
        {
            await PowerCmd.Apply(
                choiceContext, power, enemy, amount, state.Applier, cardSource, true);
        }
    }
}
