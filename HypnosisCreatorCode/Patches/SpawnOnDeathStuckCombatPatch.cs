using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 撃破時に増援を出す本家パワー（アックスボットの在庫・寄生・不意打ち）は、
/// 増援を出す前提で「戦闘終了を止める」を常に true で返す。
/// 撃破処理（ダメージ後フック〜<see cref="CreatureCmd.Kill(IReadOnlyCollection{Creature}, bool)"/> の死亡フック）の途中で
/// 例外が出ると、本家の ActionExecutor は例外を記録して続行するため、増援が出ないまま死んだ持ち主にパワーが残り、
/// 敵のいない戦闘が終わらなくなる（2026-10 報告: アックスボット撃破後に2体目が降りず、ターンだけ進む）。
/// 持ち主が死んでいて撃破処理も走っていないなら、増援の見込みは無いので戦闘終了を止めない。
/// </summary>
[HarmonyPatch]
public static class SpawnOnDeathStuckCombatPatch
{
    private static readonly ConditionalWeakTable<PowerModel, object> Warned = new();

    static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.DeclaredMethod(typeof(StockPower), nameof(PowerModel.ShouldStopCombatFromEnding))!;
        yield return AccessTools.DeclaredMethod(typeof(InfestedPower), nameof(PowerModel.ShouldStopCombatFromEnding))!;
        yield return AccessTools.DeclaredMethod(typeof(SurprisePower), nameof(PowerModel.ShouldStopCombatFromEnding))!;
    }

    static void Postfix(PowerModel __instance, ref bool __result)
    {
        if (!__result) return;
        if (__instance.Owner is not { IsDead: true } owner) return;
        // 撃破処理の途中（プレイヤー選択待ちで中断中など）は本家どおり待つ
        if (KillInProgressTracker.IsInProgress(owner)) return;

        __result = false;
        if (Warned.TryGetValue(__instance, out _)) return;
        Warned.Add(__instance, new object());
        MainFile.Logger.Warn(
            $"{__instance.GetType().Name} left on dead {owner.Monster?.Id.Entry ?? owner.ModelId.Entry}: " +
            "death handling did not finish, so combat is allowed to end");
    }
}

/// <summary>撃破処理（<see cref="CreatureCmd.Kill(IReadOnlyCollection{Creature}, bool)"/>）が走っている最中のクリーチャー。</summary>
[HarmonyPatch]
public static class KillInProgressTracker
{
    private sealed class Counter
    {
        public int Value;
    }

    private static readonly ConditionalWeakTable<Creature, Counter> InProgress = new();

    public static bool IsInProgress(Creature creature) =>
        InProgress.TryGetValue(creature, out var counter) && counter.Value > 0;

    static MethodBase TargetMethod() =>
        AccessTools.DeclaredMethod(typeof(CreatureCmd), nameof(CreatureCmd.Kill),
            [typeof(IReadOnlyCollection<Creature>), typeof(bool)])!;

    static void Prefix(IReadOnlyCollection<Creature> creatures, out List<Creature> __state)
    {
        __state = creatures.Where(c => c != null).ToList();
        foreach (var creature in __state)
            InProgress.GetValue(creature, static _ => new Counter()).Value++;
    }

    static void Postfix(ref Task __result, List<Creature> __state)
    {
        __result = ReleaseAfterAsync(__result, __state);
    }

    private static async Task ReleaseAfterAsync(Task original, List<Creature> creatures)
    {
        try
        {
            await original;
        }
        finally
        {
            foreach (var creature in creatures)
            {
                if (InProgress.TryGetValue(creature, out var counter) && counter.Value > 0)
                    counter.Value--;
            }
        }
    }
}
