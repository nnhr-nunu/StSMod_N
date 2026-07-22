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
[HarmonyPatch(typeof(NCardGrid), "InitGrid", [])]
public static class CardLibraryOrphanCleanupPatch
{
    private static readonly FieldInfo? CardRowsField =
        AccessTools.Field(typeof(NCardGrid), "_cardRows");

    private static readonly FieldInfo? ScrollContainerField =
        AccessTools.Field(typeof(NCardGrid), "_scrollContainer");

    private static readonly MethodInfo? IsCardLibraryGetter =
        AccessTools.PropertyGetter(typeof(NCardGrid), "IsCardLibrary");

    public static void Postfix(NCardGrid __instance)
    {
        // 報酬画面など他の NCardGrid には絶対に掛けない
        if (!IsLibraryGrid(__instance)) return;
        CleanupOrphans(__instance);
    }

    private static bool IsLibraryGrid(NCardGrid grid)
    {
        if (grid is NCardLibraryGrid) return true;
        if (IsCardLibraryGetter == null) return false;
        try
        {
            return (bool)IsCardLibraryGetter.Invoke(grid, null)!;
        }
        catch
        {
            return false;
        }
    }

    private static void CleanupOrphans(NCardGrid grid)
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

        foreach (var child in scroll.GetChildren())
        {
            if (child is not NGridCardHolder holder) continue;
            if (tracked.Contains(holder)) continue;

            if (GodotObject.IsInstanceValid(holder))
            {
                holder.Visible = false;
                GodotTreeExtensions.QueueFreeSafely(holder);
            }
        }
    }
}
