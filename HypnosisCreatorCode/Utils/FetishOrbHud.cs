using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 敵頭上に性癖スロットを本家オーブ（NOrb）で可視化する。
/// デフォルト1スロット。性癖が増えると空スロットも増えて並ぶ。
/// </summary>
public static class FetishOrbHud
{
    private const string NodeName = "HcFetishOrbHud";
    private const float SlotGap = 6f;
    private const float HeadOffsetY = -8f;
    private const float OrbScale = 0.72f;
    private const int MaxRetries = 40;
    private const double RetrySeconds = 0.05;

    public static void QueueRefresh(Creature creature, bool visible) =>
        QueueRefresh(creature, visible, MaxRetries);

    private static void QueueRefresh(Creature creature, bool visible, int attemptsLeft)
    {
        Callable.From(() =>
        {
            if (TryRefresh(creature, visible)) return;
            if (!visible || attemptsLeft <= 0) return;

            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null) return;

            var timer = tree.CreateTimer(RetrySeconds);
            timer.Timeout += () => QueueRefresh(creature, visible, attemptsLeft - 1);
        }).CallDeferred();
    }

    public static void Refresh(Creature creature, bool visible) =>
        TryRefresh(creature, visible);

    private static bool TryRefresh(Creature creature, bool visible)
    {
        if (creature is not { IsEnemy: true }) return true;

        var room = NCombatRoom.Instance;
        if (room == null) return false;

        var creatureNode = room.GetCreatureNode(creature);
        if (creatureNode == null || !GodotObject.IsInstanceValid(creatureNode)) return false;

        var existing = creatureNode.GetNodeOrNull<Control>(NodeName);
        if (!visible)
        {
            existing?.QueueFree();
            return true;
        }

        // ノード生成直後はヒットボックス未設定のことがある
        if (creatureNode.Hitbox == null) return false;

        var state = EnemyFetishSlots.Get(creature);
        var capacity = Math.Max(EnemyFetishSlots.DefaultCapacity, state.Capacity);
        var hud = existing ?? CreateHud(creatureNode);
        RebuildSlots(hud, state, capacity);
        PlaceAboveHead(creatureNode, hud, capacity);
        return true;
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
            Alignment = BoxContainer.AlignmentMode.Center,
            ZIndex = 40
        };
        hud.AddThemeConstantOverride("separation", (int)SlotGap);
        creatureNode.AddChild(hud);
        return hud;
    }

    private static void RebuildSlots(Control hud, FetishSlotState state, int capacity)
    {
        foreach (var child in hud.GetChildren())
            child.QueueFree();

        for (var i = 0; i < capacity; i++)
        {
            OrbModel? fetish = i < state.Fetishes.Count ? state.Fetishes[i] : null;
            hud.AddChild(CreateOrbSlot(fetish));
        }
    }

    private static Control CreateOrbSlot(OrbModel? fetish)
    {
        // 本家オーブUIを流用（空スロット / 性癖入り）
        var orb = fetish != null
            ? NOrb.Create(isLocal: false, fetish)
            : NOrb.Create(isLocal: false);

        orb.MouseFilter = Control.MouseFilterEnum.Ignore;
        orb.Scale = new Vector2(OrbScale, OrbScale);
        orb.FocusMode = Control.FocusModeEnum.None;
        return orb;
    }

    private static void PlaceAboveHead(NCreature creatureNode, Control hud, int capacity)
    {
        Vector2 anchor;
        var marker = creatureNode.Visuals?.OrbPosition;
        if (marker != null && GodotObject.IsInstanceValid(marker))
            anchor = marker.GlobalPosition;
        else
            anchor = creatureNode.GetTopOfHitbox();

        var local = creatureNode.MakeCanvasPositionLocal(anchor);
        // NOrb の実寸はテーマ依存のため、目安幅で中央揃え
        var approxSlot = 42f * OrbScale;
        var width = Math.Max(1, capacity) * approxSlot + Math.Max(0, capacity - 1) * SlotGap;
        hud.Position = new Vector2(local.X - width * 0.5f, local.Y + HeadOffsetY);
        hud.Visible = true;
    }
}
