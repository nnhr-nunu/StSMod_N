using System.Runtime.CompilerServices;
using Godot;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 希少な心臓の右クリック入力。Godot シグナルとインベントリ単位のヒットテストの両方で拾う。
/// </summary>
internal static class HeartRelicInput
{
    private static readonly ConditionalWeakTable<NRelicInventoryHolder, object> Wired = new();

    public static void Wire(NRelicInventoryHolder? holder)
    {
        if (holder == null || !Wired.TryAdd(holder, null!)) return;

        holder.GuiInput += evt => TryHandle(holder, evt);
        holder.MousePressed += evt => TryHandle(holder, evt);
    }

    public static void WireAll(NRelicInventory? inventory)
    {
        if (inventory == null) return;
        foreach (var holder in inventory.RelicNodes)
            Wire(holder);
    }

    /// <summary>インベントリ全体の _Input から、座標でホルダーを当てて発動する。</summary>
    public static bool TryHandleInventoryInput(NRelicInventory inventory, InputEvent @event)
    {
        if (!IsRightPress(@event)) return false;
        if (CombatManager.Instance is not { IsInProgress: true }) return false;

        var pos = (@event as InputEventMouse)?.GlobalPosition;
        if (pos == null) return false;

        foreach (var holder in inventory.RelicNodes)
        {
            if (!holder.Visible) continue;
            if (!holder.GetGlobalRect().HasPoint(pos.Value)) continue;
            if (!TryHandle(holder, @event)) continue;

            inventory.GetViewport()?.SetInputAsHandled();
            return true;
        }

        return false;
    }

    public static bool TryHandle(NRelicInventoryHolder holder, InputEvent @event)
    {
        if (!IsRightPress(@event)) return false;

        if (!HeartRelicActivation.TryBeginFromHolder(holder)) return false;

        holder.AcceptEvent();
        holder.GetViewport()?.SetInputAsHandled();
        return true;
    }

    private static bool IsRightPress(InputEvent @event) =>
        @event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right };
}
