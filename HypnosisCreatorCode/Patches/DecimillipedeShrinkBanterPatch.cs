using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>万足ムカデへ縮小を付与したときのプレイヤー吹き出し（本家・mod 共通）。</summary>
[HarmonyPatch]
public static class DecimillipedeShrinkBanterPatch
{
    private static MethodBase TargetMethod() =>
        typeof(PowerCmd).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m is { Name: nameof(PowerCmd.Apply), IsGenericMethodDefinition: true }
                        && m.GetParameters().Length == 6
                        && m.GetParameters()[1].ParameterType == typeof(Creature));

    public static void Postfix(Creature target, Creature applier, MethodBase __originalMethod)
    {
        if (__originalMethod is not MethodInfo method || !method.IsGenericMethod) return;
        if (method.GetGenericArguments()[0] != typeof(ShrinkPower)) return;
        if (applier is not { IsPlayer: true }) return;
        if (!DecimillipedeBanter.IsDecimillipede(target)) return;

        DecimillipedeBanter.TryShowStillMovingBanter(applier);
    }
}
