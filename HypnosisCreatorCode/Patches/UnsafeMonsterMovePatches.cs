using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// Crusher / Rocket 向け: SetMoveImmediate を使わずにスタン・スライム催眠の行動上書きを実現する。
/// 睡眠は全敵で PerformMove スキップ（<see cref="ForcedSleepVisualPower"/>）。
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
            __result = RunSlimeReplaceAsync(__instance, slime);
            return false;
        }

        var sleep = creature.GetPower<ForcedSleepVisualPower>();
        if (sleep is { ShouldSkipPerform: true })
        {
            // 睡眠中は毎ターンスキップ。起床時に ForcedSleep 側で RollMove する。
            __result = RunSkipAsync(__instance, rollNext: false);
            return false;
        }

        if (IntentOverwriteUnsafeMonsters.TryConsumeSkip(creature))
        {
            // スタン等の1回スキップ後は次行動をロールする
            __result = RunSkipAsync(__instance, rollNext: true);
            return false;
        }

        return true;
    }

    private static async Task RunSlimeReplaceAsync(MonsterModel monster, SlimeHypnosisPower slime)
    {
        await slime.TryReplacePerformAsync();
        UnsafeMonsterMoveCompletion.AfterSubstitutedPerform(monster, rollNext: true);
    }

    private static async Task RunSkipAsync(MonsterModel monster, bool rollNext)
    {
        await Task.CompletedTask;
        UnsafeMonsterMoveCompletion.AfterSubstitutedPerform(monster, rollNext);
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
            UnsafeMonsterMoveCompletion.HideIntentImmediate(__instance);
            __result = Task.CompletedTask;
            return false;
        }

        var sleep = creature.GetPower<ForcedSleepVisualPower>();
        if (sleep is { ShouldSkipPerform: true })
        {
            UnsafeMonsterMoveCompletion.HideIntentImmediate(__instance);
            __result = Task.CompletedTask;
            return false;
        }

        if (IntentOverwriteUnsafeMonsters.HasPendingSkip(creature))
        {
            UnsafeMonsterMoveCompletion.HideIntentImmediate(__instance);
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
