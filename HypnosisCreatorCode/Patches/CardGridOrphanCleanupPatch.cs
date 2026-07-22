using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 本家 NGridCardHolder のプール回収が Visible を落とさないため、
/// カードライブラリ再構築（レア絞り込み等）で枠がツリーに残ると画面左上(0,0)に浮いて見える。
/// </summary>
internal static class CardGridOrphanCleanup
{
    private static readonly FieldInfo? CardRowsField =
        AccessTools.Field(typeof(NCardGrid), "_cardRows");

    private static readonly FieldInfo? ScrollContainerField =
        AccessTools.Field(typeof(NCardGrid), "_scrollContainer");

    public static void HideFreedHolder(NGridCardHolder holder)
    {
        if (!GodotObject.IsInstanceValid(holder)) return;
        holder.Visible = false;
        holder.Position = Vector2.Zero;
        holder.MouseFilter = Control.MouseFilterEnum.Ignore;
    }

    /// <summary>
    /// scrollContainer 直下にあるが _cardRows に載っていないホルダーを回収する。
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

        foreach (var child in scroll.GetChildren())
        {
            if (child is not NGridCardHolder holder) continue;
            if (tracked.Contains(holder)) continue;

            HideFreedHolder(holder);
            GodotTreeExtensions.QueueFreeSafely(holder);
        }
    }
}

/// <summary>プール回収時に必ず非表示へ（本家 OnFreedToPool が空なのが穴）。</summary>
[HarmonyPatch(typeof(NGridCardHolder), "OnFreedToPool")]
public static class GridCardHolderFreedToPoolPatch
{
    public static void Postfix(NGridCardHolder __instance) =>
        CardGridOrphanCleanup.HideFreedHolder(__instance);
}

/// <summary>
/// 本家 OnReturnedFromPool は Position=(0,0) かつ Visible=true にする。
/// 親から外しきれない瞬間や再配置前に左上へ描画されるため、配置役が Visible を立てるまで隠す。
/// </summary>
[HarmonyPatch(typeof(NGridCardHolder), "OnReturnedFromPool")]
public static class GridCardHolderReturnedFromPoolPatch
{
    public static void Postfix(NGridCardHolder __instance)
    {
        if (!GodotObject.IsInstanceValid(__instance)) return;
        __instance.Visible = false;
    }
}

/// <summary>グリッド再構築後に、行リスト外の取り残し枠を掃除する。</summary>
[HarmonyPatch(typeof(NCardGrid), "InitGrid", [])]
public static class CardGridInitOrphanCleanupPatch
{
    public static void Postfix(NCardGrid __instance)
    {
        CardGridOrphanCleanup.CleanupOrphans(__instance);
        // RemoveChild / Free が遅延のとき用に1フレーム後も掃除
        var grid = __instance;
        Callable.From(() =>
        {
            if (GodotObject.IsInstanceValid(grid))
                CardGridOrphanCleanup.CleanupOrphans(grid);
        }).CallDeferred();
    }
}
