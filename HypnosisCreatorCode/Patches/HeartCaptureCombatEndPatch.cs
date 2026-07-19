using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 植物寄生など「戦闘終了時に心臓」予約を、追加レリック報酬として解決する。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatEnd))]
public static class HeartCaptureCombatEndPatch
{
    public static void Postfix(IRunState runState, ICombatState? combatState, CombatRoom room)
    {
        HeartCapture.FlushPending();
    }
}
