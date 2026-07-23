using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 希少な心臓の右クリック入力（GuiInput・インベントリ _Input・シグナル配線）。
/// e631028 の <c>NRelicInventoryHolder._GuiInput</c> / <c>EmitSignalMousePressed</c> パッチは
/// Harmony 適用失敗でクラス全体が無効化されていたため、ここに集約する。
/// </summary>
[HarmonyPatch]
public static class HeartRelicInputPatch
{
    [HarmonyPatch(typeof(NRelicInventory), "Add")]
    [HarmonyPostfix]
    public static void AddPostfix(NRelicInventory __instance)
    {
        var nodes = __instance.RelicNodes;
        if (nodes.Count == 0) return;
        HeartRelicInput.Wire(nodes[^1]);
    }

    [HarmonyPatch(typeof(NRelicInventory), nameof(NRelicInventory.Initialize))]
    [HarmonyPostfix]
    public static void InitializePostfix(NRelicInventory __instance) =>
        HeartRelicInput.WireAll(__instance);

    [HarmonyPatch(typeof(NRelicInventoryHolder), "_Ready")]
    [HarmonyPostfix]
    public static void HolderReadyPostfix(NRelicInventoryHolder __instance) =>
        HeartRelicInput.Wire(__instance);

    [HarmonyPatch(typeof(NRelicInventory), "_Input")]
    [HarmonyPrefix]
    public static bool InventoryInputPrefix(NRelicInventory __instance, InputEvent @event)
    {
        if (!HeartRelicInput.TryHandleInventoryInput(__instance, @event)) return true;
        return false;
    }

    [HarmonyPatch(typeof(NClickableControl), "_GuiInput")]
    [HarmonyPrefix]
    public static bool ClickableGuiInputPrefix(NClickableControl __instance, InputEvent @event)
    {
        if (__instance is not NRelicInventoryHolder holder) return true;
        if (!HeartRelicInput.TryHandle(holder, @event)) return true;

        holder.AcceptEvent();
        return false;
    }

    [HarmonyPatch(typeof(NButton), "_Input")]
    [HarmonyPrefix]
    public static bool ButtonInputPrefix(NButton __instance, InputEvent @event)
    {
        if (__instance is not NRelicInventoryHolder holder) return true;
        if (!HeartRelicInput.TryHandle(holder, @event)) return true;

        holder.AcceptEvent();
        __instance.GetViewport()?.SetInputAsHandled();
        return false;
    }
}
