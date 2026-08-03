using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 敵ターン中に silent 追加した捨て札カードのプレビューを、行動完了後の安全なタイミングで表示する。
/// </summary>
[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.PerformMove))]
public static class DeferredDiscardPilePreviewPerformMovePatch
{
    public static void Postfix(ref Task __result) =>
        __result = FlushAfterAsync(__result);

    private static async Task FlushAfterAsync(Task original)
    {
        await original;
        await PendingDiscardPileCardPreview.FlushIfAnyAsync();
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterSideTurnEnd))]
public static class DeferredDiscardPilePreviewEnemyTurnEndPatch
{
    public static void Postfix(ref Task __result, CombatSide side)
    {
        if (side != CombatSide.Enemy) return;

        var original = __result;
        __result = ContinueAsync(original);
    }

    private static async Task ContinueAsync(Task original)
    {
        await original;
        await PendingDiscardPileCardPreview.FlushIfAnyAsync();
    }
}
