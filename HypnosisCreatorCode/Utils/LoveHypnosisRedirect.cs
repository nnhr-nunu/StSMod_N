using System.Reflection;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>好き好き催眠 — 敵のバフ／ブロック付与をプレイヤーへ付け替える判定。</summary>
public static class LoveHypnosisRedirect
{
    private static readonly MethodInfo GenericApply = typeof(PowerCmd)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m is { Name: nameof(PowerCmd.Apply), IsGenericMethodDefinition: true }
                    && m.GetParameters().Length == 6
                    && m.GetParameters()[1].ParameterType == typeof(Creature));

    public static bool ShouldStealEnemyBuff(Creature? applier, Creature target, PowerModel power, out Creature player)
    {
        player = null!;
        if (applier == null) return false;
        if (target.Side != CombatSide.Enemy) return false;
        if (power.Type != PowerType.Buff) return false;
        if (!TryGetActiveLoveHypnosis(applier, out var hypnosis) || !hypnosis.StealBuff) return false;

        var resolved = hypnosis.ResolvePlayerCreature();
        if (resolved is not { IsAlive: true }) return false;

        player = resolved;
        return true;
    }

    public static bool ShouldStealEnemyBuffAmount(PowerModel power, decimal offset, out Creature player)
    {
        player = null!;
        if (offset <= 0m) return false;
        if (power.Type != PowerType.Buff) return false;
        if (power.Owner is not { Side: CombatSide.Enemy } owner) return false;
        if (!TryGetActiveLoveHypnosis(owner, out var hypnosis) || !hypnosis.StealBuff) return false;

        var resolved = hypnosis.ResolvePlayerCreature();
        if (resolved is not { IsAlive: true }) return false;

        player = resolved;
        return true;
    }

    /// <summary>
    /// 敵への付与が完了したあと、同量を敵から剥がしてプレイヤーへ移す。
    /// Prefix で target を差し替えると敵行動中の CustomScaledWait で進行不能になるため、Postfix 奪取のみ使う。
    /// </summary>
    public static async Task TransferBuffToPlayer(
        PlayerChoiceContext choiceContext,
        PowerModel enemyPower,
        decimal amount,
        Creature player,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amount <= 0m) return;

        var remove = (int)Math.Min(amount, enemyPower.Amount);
        if (remove > 0)
            await PowerCmd.ModifyAmount(choiceContext, enemyPower, -remove, applier, cardSource, silent: true);

        var apply = GenericApply.MakeGenericMethod(enemyPower.GetType());
        var task = (Task)apply.Invoke(null, [choiceContext, player, amount, applier, cardSource, true])!;
        await task;
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
