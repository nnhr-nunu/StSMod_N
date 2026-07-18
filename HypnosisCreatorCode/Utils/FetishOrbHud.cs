using Godot;
using HypnosisCreator.HypnosisCreatorCode.Orbs;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 敵の下部（バフ／デバフ行付近）に性癖スロットをオーブ風アイコンで可視化する。
/// 頭上だと行動予定と重なるため、Hitbox 下辺基準で配置する。
/// （本家 NOrb はプレイヤー専用前提のため、同見た目の Control で描画）
/// </summary>
public static class FetishOrbHud
{
    private const string NodeName = "HcFetishOrbHud";
    private const float SlotSize = 32f;
    private const float SlotGap = 3f;
    /// <summary>Hitbox 下辺からのオフセット。負で少し上（HP／Power 行のすぐ上付近）。</summary>
    private const float FeetOffsetY = -36f;
    private const int MaxRetries = 60;
    private const double RetrySeconds = 0.05;

    public static void QueueRefresh(Creature creature, bool visible) =>
        QueueRefresh(creature, visible, MaxRetries);

    private static void QueueRefresh(Creature creature, bool visible, int attemptsLeft)
    {
        Callable.From(() =>
        {
            try
            {
                if (TryRefresh(creature, visible)) return;
            }
            catch (Exception e)
            {
                MainFile.Logger.Warn($"FetishOrbHud refresh error: {e.Message}");
            }

            if (!visible || attemptsLeft <= 0)
            {
                if (attemptsLeft <= 0)
                    MainFile.Logger.Warn($"FetishOrbHud: give up for {creature.Name}");
                return;
            }

            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null) return;
            tree.CreateTimer(RetrySeconds).Timeout += () =>
                QueueRefresh(creature, visible, attemptsLeft - 1);
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
        if (creatureNode.Hitbox == null) return false;

        var existing = creatureNode.GetNodeOrNull<Control>(NodeName);
        if (!visible)
        {
            existing?.QueueFree();
            return true;
        }

        var state = EnemyFetishSlots.Get(creature);
        var capacity = Math.Max(EnemyFetishSlots.DefaultCapacity, state.Capacity);
        var hud = existing ?? CreateHud(creatureNode);
        RebuildSlots(hud, state, capacity);
        PlaceBelowBody(creatureNode, hud, capacity);
        hud.Visible = true;
        hud.Modulate = Colors.White;
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
            ZIndex = 80,
            ZAsRelative = false
        };
        hud.AddThemeConstantOverride("separation", (int)SlotGap);
        creatureNode.AddChild(hud);
        // 最後尾にして他UIの下に潜らないようにする
        creatureNode.MoveChild(hud, creatureNode.GetChildCount() - 1);
        return hud;
    }

    private static void RebuildSlots(Control hud, FetishSlotState state, int capacity)
    {
        foreach (var child in hud.GetChildren())
            child.QueueFree();

        for (var i = 0; i < capacity; i++)
        {
            OrbModel? fetish = i < state.Fetishes.Count ? state.Fetishes[i] : null;
            hud.AddChild(CreateSlot(fetish));
        }
    }

    private static Control CreateSlot(OrbModel? fetish)
    {
        var root = new Control
        {
            CustomMinimumSize = new Vector2(SlotSize, SlotSize),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            TooltipText = fetish?.Title.GetFormattedText() ?? "空の性癖スロット"
        };

        var color = fetish is HypnosisCreatorOrb hc
            ? hc.AccentColor
            : new Color(0.15f, 0.15f, 0.2f, 0.85f);

        // 背景ディスク
        var disc = new ColorRect
        {
            Color = color,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        disc.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(disc);

        // 枠
        var border = new Panel
        {
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        var style = new StyleBoxFlat
        {
            BgColor = Colors.Transparent,
            BorderColor = Colors.White,
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 999,
            CornerRadiusTopRight = 999,
            CornerRadiusBottomLeft = 999,
            CornerRadiusBottomRight = 999
        };
        border.AddThemeStyleboxOverride("panel", style);
        border.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(border);

        // オーブ画像があれば乗せる
        if (fetish is HypnosisCreatorOrb orbModel)
        {
            var path = orbModel.CustomIconPath;
            if (!string.IsNullOrEmpty(path) && ResourceLoader.Exists(path))
            {
                var tex = ResourceLoader.Load<Texture2D>(path);
                if (tex != null)
                {
                    var icon = new TextureRect
                    {
                        Texture = tex,
                        ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                        StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                        MouseFilter = Control.MouseFilterEnum.Ignore
                    };
                    icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
                    icon.OffsetLeft = 4;
                    icon.OffsetTop = 4;
                    icon.OffsetRight = -4;
                    icon.OffsetBottom = -4;
                    root.AddChild(icon);
                }
            }
        }

        // 短いラベル（画像が無いときも識別可能）
        var label = new Label
        {
            Text = ShortLabel(fetish),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", 11);
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeConstantOverride("outline_size", 3);
        label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(label);

        return root;
    }

    private static string ShortLabel(OrbModel? fetish)
    {
        if (fetish == null) return "·";
        var title = fetish.Title.GetFormattedText();
        if (string.IsNullOrWhiteSpace(title) || title.Contains(".title", StringComparison.Ordinal))
        {
            return FetishCombat.ToFetishType(fetish) switch
            {
                FetishType.Sm => "SM",
                FetishType.DomSub => "DS",
                FetishType.Abnormal => "Ab",
                FetishType.Trance => "Tr",
                _ => "?"
            };
        }

        return title.Length <= 3 ? title : title[..2];
    }

    private static void PlaceBelowBody(NCreature creatureNode, Control hud, int capacity)
    {
        var bottom = creatureNode.GetBottomOfHitbox();
        var local = creatureNode.MakeCanvasPositionLocal(bottom);
        var width = Math.Max(1, capacity) * SlotSize + Math.Max(0, capacity - 1) * SlotGap;
        // バフ／デバフ行と同帯に寄せ、意図アイコン（頭上）と分離する
        hud.Position = new Vector2(local.X - width * 0.5f, local.Y + FeetOffsetY);
        hud.Size = new Vector2(width, SlotSize);
    }
}
