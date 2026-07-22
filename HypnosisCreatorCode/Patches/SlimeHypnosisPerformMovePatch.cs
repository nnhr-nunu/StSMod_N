using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// Crusher / Rocket など意図ステート書き換えが危険な敵向け。
/// <see cref="SlimeHypnosisPower"/> が付いているとき、本家行動の代わりに粘液付与を実行する。
/// </summary>
[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.PerformMove))]
public static class SlimeHypnosisPerformMovePatch
{
    public static bool Prefix(MonsterModel __instance, ref Task __result)
    {
        var creature = __instance.Creature;
        var power = creature?.GetPower<SlimeHypnosisPower>();
        if (power is not { ShouldReplacePerform: true })
            return true;

        __result = power.TryReplacePerformAsync();
        return false;
    }
}

/// <summary>
/// 意図アイコン演出（攻撃モーション等）もスキップし、粘液差し替えとの見た目ズレを減らす。
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.PerformIntent))]
public static class SlimeHypnosisPerformIntentPatch
{
    public static bool Prefix(NCreature __instance, ref Task __result)
    {
        var power = __instance.Entity?.GetPower<SlimeHypnosisPower>();
        if (power is not { ShouldReplacePerform: true })
            return true;

        // 粘液付与は PerformMove 側で行う。ここでは演出だけ止める。
        __result = Task.CompletedTask;
        return false;
    }
}
