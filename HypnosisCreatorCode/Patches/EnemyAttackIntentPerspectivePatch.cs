using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 意図ダメージ読取り中はカード所有者を LocalContext の「自分」として扱う。
/// </summary>
[HarmonyPatch]
public static class EnemyAttackIntentPerspectivePatch
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (var method in typeof(LocalContext).GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            if (method.Name != "GetMe") continue;
            if (method.ReturnType != typeof(Player)) continue;
            yield return method;
        }
    }

    static bool Prefix(ref Player __result)
    {
        if (!EnemyAttackIntentPerspective.TryPeek(out var player)) return true;

        __result = player;
        return false;
    }
}
