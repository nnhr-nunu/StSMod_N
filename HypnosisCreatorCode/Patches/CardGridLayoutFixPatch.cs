using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 本家 NCardGrid の仮想スクロール不具合対策。
/// 末尾行が列数未満のまま再利用されると、インデックス計算がずれて
/// 「1枚だけ位置がおかしい」「絞り込み後にスクロールがおかしい」になる。
/// </summary>
internal static class CardGridLayoutFix
{
    private static readonly FieldInfo CardRowsField =
        AccessTools.Field(typeof(NCardGrid), "_cardRows");

    private static readonly FieldInfo CardsField =
        AccessTools.Field(typeof(NCardGrid), "_cards");

    private static readonly FieldInfo PileTypeField =
        AccessTools.Field(typeof(NCardGrid), "_pileType");

    private static readonly FieldInfo ScrollContainerField =
        AccessTools.Field(typeof(NCardGrid), "_scrollContainer");

    private static readonly FieldInfo SlidingWindowField =
        AccessTools.Field(typeof(NCardGrid), "_slidingWindowCardIndex");

    private static readonly FieldInfo TargetDragField =
        AccessTools.Field(typeof(NCardGrid), "_targetDrag");

    private static readonly MethodInfo GetVisibilityMethod =
        AccessTools.Method(typeof(NCardGrid), "GetCardVisibility");

    private static readonly MethodInfo UpdatePositionsMethod =
        AccessTools.Method(typeof(NCardGrid), "UpdateGridPositions");

    private static readonly MethodInfo OnHolderPressedMethod =
        AccessTools.Method(typeof(NCardGrid), "OnHolderPressed");

    private static readonly MethodInfo OnHolderAltPressedMethod =
        AccessTools.Method(typeof(NCardGrid), "OnHolderAltPressed");

    private static readonly MethodInfo GetColumnsMethod =
        AccessTools.PropertyGetter(typeof(NCardGrid), "Columns");

    /// <summary>
    /// 各行のホルダー数を Columns に揃える（不足分は非表示プレースホルダ）。
    /// </summary>
    public static void PadRowsToColumnCount(NCardGrid grid)
    {
        if (CardRowsField == null || CardsField == null || GetVisibilityMethod == null || GetColumnsMethod == null)
            return;

        if (CardRowsField.GetValue(grid) is not List<List<NGridCardHolder>> rows || rows.Count == 0)
            return;

        if (CardsField.GetValue(grid) is not List<CardModel> cards || cards.Count == 0)
            return;

        var columns = (int)GetColumnsMethod.Invoke(grid, null)!;
        if (columns <= 0)
            return;

        if (ScrollContainerField?.GetValue(grid) is not Control scrollContainer)
            return;

        var pileType = PileTypeField != null
            ? (PileType)PileTypeField.GetValue(grid)!
            : PileType.None;

        var template = cards[0];
        var visibility = (ModelVisibility)GetVisibilityMethod.Invoke(grid, [template])!;

        foreach (var row in rows)
        {
            while (row.Count < columns)
            {
                var nCard = NCard.Create(template, visibility);
                if (nCard == null)
                    return;

                var holder = NGridCardHolder.Create(nCard);
                if (holder == null)
                    return;

                ConnectHolderSignals(grid, holder);
                holder.Visible = false;
                holder.Scale = holder.SmallScale;
                holder.MouseFilter = Control.MouseFilterEnum.Stop;
                GodotTreeExtensions.AddChildSafely(scrollContainer, holder);
                nCard.UpdateVisuals(pileType, CardPreviewMode.Normal);
                row.Add(holder);
            }
        }
    }

    public static void SyncTargetDragToScroll(NCardGrid grid)
    {
        if (TargetDragField == null || ScrollContainerField == null)
            return;
        if (ScrollContainerField.GetValue(grid) is not Control scrollContainer)
            return;

        TargetDragField.SetValue(grid, scrollContainer.Position.Y);
    }

    public static void RefreshAbsolutePositions(NCardGrid grid)
    {
        if (UpdatePositionsMethod == null || SlidingWindowField == null)
            return;

        var index = (int)SlidingWindowField.GetValue(grid)!;
        UpdatePositionsMethod.Invoke(grid, [index]);
    }

    private static void ConnectHolderSignals(NCardGrid grid, NGridCardHolder holder)
    {
        if (OnHolderPressedMethod != null)
        {
            holder.Connect(
                NCardHolder.SignalName.Pressed,
                Callable.From((NCardHolder h) => OnHolderPressedMethod.Invoke(grid, [h])));
        }

        if (OnHolderAltPressedMethod != null)
        {
            holder.Connect(
                NCardHolder.SignalName.AltPressed,
                Callable.From((NCardHolder h) => OnHolderAltPressedMethod.Invoke(grid, [h])));
        }
    }
}

/// <summary>グリッド再構築後に末尾行を列数まで埋め、スクロール目標値も同期する。</summary>
[HarmonyPatch(typeof(NCardGrid), "InitGrid", [])]
public static class CardGridInitLayoutFixPatch
{
    public static void Postfix(NCardGrid __instance)
    {
        CardGridLayoutFix.PadRowsToColumnCount(__instance);
        CardGridLayoutFix.SyncTargetDragToScroll(__instance);
        CardGridLayoutFix.RefreshAbsolutePositions(__instance);
    }
}

/// <summary>上方向の行再利用後に絶対座標で再配置（相対Yだけのずれ防止）。</summary>
[HarmonyPatch(typeof(NCardGrid), "ReallocateAbove")]
public static class CardGridReallocateAboveFixPatch
{
    public static void Postfix(NCardGrid __instance)
    {
        CardGridLayoutFix.RefreshAbsolutePositions(__instance);
    }
}

/// <summary>下方向の行再利用後に絶対座標で再配置。</summary>
[HarmonyPatch(typeof(NCardGrid), "ReallocateBelow")]
public static class CardGridReallocateBelowFixPatch
{
    public static void Postfix(NCardGrid __instance)
    {
        CardGridLayoutFix.RefreshAbsolutePositions(__instance);
    }
}
