using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// Crusher / Rocket 向け: SetMoveImmediate を使わずに睡眠・スタン・スライム催眠の行動上書きを実現する。
/// </summary>
[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.PerformMove))]
public static class UnsafeMonsterPerformMovePatch
{
    public static bool Prefix(MonsterModel __instance, ref Task __result)
    {
        var creature = __instance.Creature;
        if (creature == null) return true;

        var slime = creature.GetPower<SlimeHypnosisPower>();
        if (slime is { ShouldReplacePerform: true })
        {
            __result = slime.TryReplacePerformAsync();
            return false;
        }

        var sleep = creature.GetPower<ForcedSleepVisualPower>();
        if (sleep is { ShouldSkipPerform: true })
        {
            __result = Task.CompletedTask;
            return false;
        }

        if (IntentOverwriteUnsafeMonsters.TryConsumeSkip(creature))
        {
            __result = AfterOneShotSkipAsync(__instance);
            return false;
        }

        return true;
    }

    private static async Task AfterOneShotSkipAsync(MonsterModel monster)
    {
        // スタン等で今ターンの行動を止めたあと、次ターン用に通常ロールへ戻す
        try
        {
            var creature = monster.Creature;
            if (creature is not { IsAlive: true }) return;
            var targets = creature.CombatState?.GetOpponentsOf(creature) ?? [];
            monster.RollMove(targets);
        }
        catch
        {
            // ロール失敗時はバニラ側に委ねる
        }

        await Task.CompletedTask;
    }
}

[HarmonyPatch(typeof(NCreature), nameof(NCreature.PerformIntent))]
public static class UnsafeMonsterPerformIntentPatch
{
    public static bool Prefix(NCreature __instance, ref Task __result)
    {
        var creature = __instance.Entity;
        if (creature == null) return true;

        var slime = creature.GetPower<SlimeHypnosisPower>();
        if (slime is { ShouldReplacePerform: true })
        {
            __result = Task.CompletedTask;
            return false;
        }

        var sleep = creature.GetPower<ForcedSleepVisualPower>();
        if (sleep is { ShouldSkipPerform: true })
        {
            __result = Task.CompletedTask;
            return false;
        }

        if (IntentOverwriteUnsafeMonsters.HasPendingSkip(creature))
        {
            __result = Task.CompletedTask;
            return false;
        }

        return true;
    }
}

/// <summary>
/// 本家 Stun は内部で SetMoveImmediate する。不安全敵ではスキップ予約だけ残す。
/// </summary>
[HarmonyPatch(typeof(Creature), "StunInternal")]
public static class UnsafeMonsterStunInternalPatch
{
    public static bool Prefix(Creature __instance)
    {
        if (!IntentOverwriteUnsafeMonsters.IsUnsafe(__instance)) return true;
        if (__instance.IsDead || __instance.CombatState == null) return true;

        IntentOverwriteUnsafeMonsters.ArmSkipOnce(__instance);
        return false;
    }
}
