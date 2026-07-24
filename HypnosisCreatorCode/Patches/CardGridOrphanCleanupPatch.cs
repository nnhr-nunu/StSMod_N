using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// カードライブラリ専用。報酬／選択画面の NGridCardHolder プールには触らない。
/// 絞り込み再構築後に scroll 直下へ残った枠だけ回収する。
/// </summary>
public static class CardLibraryOrphanCleanup
{
    private static readonly FieldInfo? CardRowsField =
        AccessTools.Field(typeof(NCardGrid), "_cardRows");

    private static readonly FieldInfo? ScrollContainerField =
        AccessTools.Field(typeof(NCardGrid), "_scrollContainer");

    /// <summary>
    /// scrollContainer 直下にあるが _cardRows に載っていないホルダーを回収する。
    /// tracked が空、または孤児が過半数を超えるときは誤削除防止のため何もしない。
    /// </summary>
    public static void CleanupOrphans(NCardGrid grid)
    {
        if (CardRowsField == null || ScrollContainerField == null) return;
        if (ScrollContainerField.GetValue(grid) is not Control scroll) return;
        if (CardRowsField.GetValue(grid) is not System.Collections.IEnumerable rows) return;

        var tracked = new HashSet<NGridCardHolder>();
        foreach (var rowObj in rows)
        {
            if (rowObj is not System.Collections.IEnumerable row) continue;
            foreach (var cell in row)
            {
                if (cell is NGridCardHolder holder)
                    tracked.Add(holder);
            }
        }

        if (tracked.Count == 0) return;

        var orphans = new List<NGridCardHolder>();
        foreach (var child in scroll.GetChildren())
        {
            if (child is not NGridCardHolder holder) continue;
            if (tracked.Contains(holder)) continue;
            orphans.Add(holder);
        }

        // 行リストと scroll 子の大半が不一致＝構築途中とみなし掃除しない
        if (orphans.Count > tracked.Count) return;

        foreach (var holder in orphans)
        {
            if (!GodotObject.IsInstanceValid(holder)) continue;
            GodotTreeExtensions.QueueFreeSafely(holder);
        }
    }

    public static void ScheduleCleanup(NCardGrid grid)
    {
        CleanupOrphans(grid);
        Callable.From(() =>
        {
            if (GodotObject.IsInstanceValid(grid))
                CleanupOrphans(grid);
        }).CallDeferred();
    }
}

/// <summary>
/// NCardLibraryGrid.InitGrid 完了後にだけ掃除する（base.InitGrid 直後だと行未登録で全消しになる）。
/// </summary>
[HarmonyPatch(typeof(NCardLibraryGrid), "InitGrid", [])]
public static class CardLibraryOrphanCleanupPatch
{
    public static void Postfix(NCardGrid __instance) =>
        CardLibraryOrphanCleanup.ScheduleCleanup(__instance);
}
