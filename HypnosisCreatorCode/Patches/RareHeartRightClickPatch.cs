using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 希少な心臓をレリック枠の右クリックで発動する。
/// 左クリックは本家どおり Inspect 画面。
/// </summary>
[HarmonyPatch]
public static class RareHeartRightClickPatch
{
    [HarmonyPatch(typeof(NRelicInventoryHolder), "_GuiInput")]
    [HarmonyPrefix]
    public static bool HolderGuiInputPrefix(NRelicInventoryHolder __instance, InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
            return true;

        if (!HeartRelicActivation.TryBeginFromHolder(__instance)) return true;

        __instance.AcceptEvent();
        return false;
    }

    /// <summary>基底クラス経由の入力も拾う（保険）。</summary>
    [HarmonyPatch(typeof(NClickableControl), "_GuiInput")]
    [HarmonyPrefix]
    public static bool GuiInputPrefix(NClickableControl __instance, InputEvent @event)
    {
        if (__instance is not NRelicInventoryHolder holder) return true;
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
            return true;

        if (!HeartRelicActivation.TryBeginFromHolder(holder)) return true;

        holder.AcceptEvent();
        return false;
    }

    [HarmonyPatch(typeof(NClickableControl), "EmitSignalMousePressed")]
    [HarmonyPrefix]
    public static void MousePressedPrefix(NClickableControl __instance, InputEvent @event)
    {
        if (__instance is not NRelicInventoryHolder holder) return;
        if (@event is not InputEventMouseButton { ButtonIndex: MouseButton.Right }) return;

        if (HeartRelicActivation.TryBeginFromHolder(holder))
            holder.AcceptEvent();
    }
}
