using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// カードライブラリ専用。スライディングウィンドウの行再利用時に alpha／Visible が崩れるのを防ぐ。
/// 本家 <see cref="NCardGrid.AssignCardsToRow"/> は Modulate を戻さないため、タブ切替の AnimateOut 後に
/// 見えない／左上に浮く症状が出ることがある。
/// </summary>
[HarmonyPatch(typeof(NCardGrid), "AssignCardsToRow")]
public static class CardLibraryAssignCardsVisibilityPatch
{
    private static readonly PropertyInfo? IsCardLibraryProperty =
        AccessTools.Property(typeof(NCardGrid), "IsCardLibrary");

    public static void Postfix(NCardGrid __instance, List<NGridCardHolder> row, int startIndex)
    {
        if (!IsLibraryGrid(__instance)) return;

        var cardsField = AccessTools.Field(typeof(NCardGrid), "_cards");
        if (cardsField?.GetValue(__instance) is not System.Collections.IList cards) return;

        for (var i = 0; i < row.Count; i++)
        {
            var holder = row[i];
            if (!GodotObject.IsInstanceValid(holder)) continue;

            if (startIndex + i >= cards.Count)
            {
                holder.Visible = false;
                continue;
            }

            holder.Visible = true;
            holder.MouseFilter = Control.MouseFilterEnum.Stop;
            var modulate = holder.Modulate;
            modulate.A = 1f;
            holder.Modulate = modulate;
        }
    }

    private static bool IsLibraryGrid(NCardGrid grid)
    {
        if (grid is NCardLibraryGrid) return true;
        if (IsCardLibraryProperty == null) return false;
        try
        {
            return (bool)IsCardLibraryProperty.GetValue(grid)!;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// InitGrid で QueueFree 待ちの -OLD 枠が (0,0) に一瞬残るのを隠す（ライブラリのみ）。
/// </summary>
[HarmonyPatch(typeof(NCardLibraryGrid), "InitGrid", [])]
public static class CardLibraryInitGridStrayCleanupPatch
{
    private static readonly FieldInfo? ScrollContainerField =
        AccessTools.Field(typeof(NCardGrid), "_scrollContainer");

    public static void Postfix(NCardGrid __instance)
    {
        if (ScrollContainerField?.GetValue(__instance) is not Control scroll) return;

        foreach (var child in scroll.GetChildren())
        {
            if (child is not NGridCardHolder holder) continue;
            if (!GodotObject.IsInstanceValid(holder)) continue;
            if (!holder.Name.ToString().Contains("-OLD", StringComparison.Ordinal)) continue;

            holder.Visible = false;
            holder.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
    }
}
