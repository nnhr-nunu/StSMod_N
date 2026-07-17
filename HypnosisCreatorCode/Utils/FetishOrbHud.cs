using Godot;
using HypnosisCreator.HypnosisCreatorCode.Orbs;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>敵頭上に性癖スロット（オーブ風）を表示する。</summary>
public static class FetishOrbHud
{
    private const string NodeName = "HcFetishOrbHud";
    private const float SlotSize = 28f;
    private const float SlotGap = 4f;
    private const float HeadOffsetY = -36f;

    public static void QueueRefresh(Creature creature, bool visible) =>
        Callable.From(() => Refresh(creature, visible)).CallDeferred();

    public static void Refresh(Creature creature, bool visible)
    {
        var room = NCombatRoom.Instance;
        if (room == null) return;

        var creatureNode = room.GetCreatureNode(creature);
        if (creatureNode == null || !GodotObject.IsInstanceValid(creatureNode)) return;

        var existing = creatureNode.GetNodeOrNull<Control>(NodeName);
        if (!visible)
        {
            existing?.QueueFree();
            return;
        }

        var state = EnemyFetishSlots.Get(creature);
        var hud = existing ?? CreateHud(creatureNode);
        RebuildSlots(hud, state);
        PlaceAboveHead(creatureNode, hud, state.Capacity);
    }

    public static void ClearAll()
    {
        var room = NCombatRoom.Instance;
        if (room == null) return;

        foreach (var creatureNode in room.CreatureNodes)
            creatureNode.GetNodeOrNull<Control>(NodeName)?.QueueFree();
    }

    private static Control CreateHud(NCreature creatureNode)
    {
        var hud = new HBoxContainer
        {
            Name = NodeName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Alignment = BoxContainer.AlignmentMode.Center
        };
        hud.AddThemeConstantOverride("separation", (int)SlotGap);
        creatureNode.AddChild(hud);
        return hud;
    }

    private static void RebuildSlots(Control hud, FetishSlotState state)
    {
        foreach (var child in hud.GetChildren())
            child.QueueFree();

        for (var i = 0; i < state.Capacity; i++)
        {
            OrbModel? fetish = i < state.Fetishes.Count ? state.Fetishes[i] : null;
            hud.AddChild(CreateSlot(fetish));
        }
    }

    private static Control CreateSlot(OrbModel? fetish)
    {
        var panel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(SlotSize, SlotSize),
            MouseFilter = Control.MouseFilterEnum.Stop,
            TooltipText = fetish?.Title.GetFormattedText() ?? "—"
        };

        var color = fetish is HypnosisCreatorOrb hc
            ? hc.AccentColor
            : new Color(0.2f, 0.2f, 0.25f, 0.75f);

        var style = new StyleBoxFlat
        {
            BgColor = color,
            CornerRadiusTopLeft = 999,
            CornerRadiusTopRight = 999,
            CornerRadiusBottomLeft = 999,
            CornerRadiusBottomRight = 999,
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            BorderColor = Colors.White.Darkened(0.2f)
        };
        panel.AddThemeStyleboxOverride("panel", style);

        var label = new Label
        {
            Text = ShortLabel(fetish),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", 10);
        panel.AddChild(label);
        return panel;
    }

    private static string ShortLabel(OrbModel? fetish)
    {
        if (fetish == null) return "·";
        var title = fetish.Title.GetFormattedText();
        return title.Length <= 4 ? title : title[..3];
    }

    private static void PlaceAboveHead(NCreature creatureNode, Control hud, int capacity)
    {
        var top = creatureNode.GetTopOfHitbox();
        var local = creatureNode.MakeCanvasPositionLocal(top);
        var width = Math.Max(1, capacity) * SlotSize + Math.Max(0, capacity - 1) * SlotGap;
        hud.Position = new Vector2(local.X - width * 0.5f, local.Y + HeadOffsetY);
    }
}
