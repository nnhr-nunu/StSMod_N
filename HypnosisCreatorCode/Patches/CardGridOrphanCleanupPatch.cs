using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// カードライブラリ再構築中フラグ。プール返却直後の (0,0) 誤描画を防ぐ（報酬画面には掛けない）。
/// </summary>
internal static class CardLibraryGridBuildScope
{
    public static bool Active { get; private set; }

    public static void Enter() => Active = true;

    public static void Exit() => Active = false;
}

/// <summary>
/// コレクション（カードライブラリ）専用の枠表示補正。
/// 過去対応（afea14a）: プール返却で (0,0)・Visible=true になる穴を塞ぐ。
/// 34617de 以降: 報酬画面を壊さないようライブラリ構築中だけ非表示にする。
/// QueueFree は使わず誤削除を防ぐ（行未登録時の全消し再発防止）。
/// </summary>
internal static class CardLibraryPresentation
{
    private static readonly FieldInfo? CardRowsField =
        AccessTools.Field(typeof(NCardGrid), "_cardRows");

    private static readonly FieldInfo? ScrollContainerField =
        AccessTools.Field(typeof(NCardGrid), "_scrollContainer");

    public static void SyncAfterInit(NCardGrid grid)
    {
        EnsureTrackedVisible(grid);
        HideOrphans(grid);

        var captured = grid;
        Callable.From(() =>
        {
            if (GodotObject.IsInstanceValid(captured))
            {
                EnsureTrackedVisible(captured);
                HideOrphans(captured);
            }
        }).CallDeferred();
    }

    private static HashSet<NGridCardHolder> CollectTracked(NCardGrid grid)
    {
        var tracked = new HashSet<NGridCardHolder>();
        if (CardRowsField?.GetValue(grid) is not System.Collections.IEnumerable rows)
            return tracked;

        foreach (var rowObj in rows)
        {
            if (rowObj is not System.Collections.IEnumerable row) continue;
            foreach (var cell in row)
            {
                if (cell is NGridCardHolder holder)
                    tracked.Add(holder);
            }
        }

        return tracked;
    }

    private static void EnsureTrackedVisible(NCardGrid grid)
    {
        foreach (var holder in CollectTracked(grid))
        {
            if (!GodotObject.IsInstanceValid(holder)) continue;
            holder.Visible = true;
            holder.MouseFilter = Control.MouseFilterEnum.Stop;
        }
    }

    private static void HideOrphans(NCardGrid grid)
    {
        if (ScrollContainerField?.GetValue(grid) is not Control scroll) return;

        var tracked = CollectTracked(grid);
        if (tracked.Count == 0) return;

        foreach (var child in scroll.GetChildren())
        {
            if (child is not NGridCardHolder holder) continue;
            if (tracked.Contains(holder)) continue;
            if (!GodotObject.IsInstanceValid(holder)) continue;

            holder.Visible = false;
            holder.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
    }
}

[HarmonyPatch(typeof(NCardLibraryGrid), "InitGrid", [])]
public static class CardLibraryGridInitPatch
{
    public static void Prefix() => CardLibraryGridBuildScope.Enter();

    public static void Postfix(NCardGrid __instance)
    {
        try
        {
            CardLibraryPresentation.SyncAfterInit(__instance);
        }
        finally
        {
            CardLibraryGridBuildScope.Exit();
        }
    }
}

/// <summary>
/// ライブラリ構築中にプールから取り出された枠は配置まで非表示（本家 OnReturnedFromPool は Visible=true）。
/// </summary>
[HarmonyPatch(typeof(NGridCardHolder), nameof(NGridCardHolder.OnReturnedFromPool))]
public static class CardLibraryPoolReturnPatch
{
    public static void Postfix(NGridCardHolder __instance)
    {
        if (!CardLibraryGridBuildScope.Active) return;
        if (!GodotObject.IsInstanceValid(__instance)) return;
        __instance.Visible = false;
    }
}
