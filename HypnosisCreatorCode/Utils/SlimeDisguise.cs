using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// スライム催眠用。スライム系モンスターからランダムに見た目・表示名を差し替える。
/// </summary>
public static class SlimeDisguise
{
    private static readonly Func<MonsterModel>[] Factories =
    [
        ModelDb.Monster<LeafSlimeS>,
        ModelDb.Monster<LeafSlimeM>,
        ModelDb.Monster<TwigSlimeS>,
        ModelDb.Monster<TwigSlimeM>,
        ModelDb.Monster<SlimedBerserker>
    ];

    private static readonly FieldInfo? VisualsField = typeof(NCreature).GetField(
        "<Visuals>k__BackingField",
        BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? SpineAnimatorField = typeof(NCreature).GetField(
        "_spineAnimator",
        BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo? ConnectSpineMethod = typeof(NCreature).GetMethod(
        "ConnectSpineAnimatorSignals",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    public sealed class State
    {
        public required string DisplayName { get; init; }
        public required MonsterModel Disguise { get; init; }
        public NCreatureVisuals? OriginalVisuals { get; init; }
        public NCreatureVisuals? DisguiseVisuals { get; init; }
        /// <summary>左配置の敵向けに Scale.X を反転したとき true。</summary>
        public bool FlippedForLeftSide { get; init; }
    }

    public static State? Apply(Creature creature, Rng rng)
    {
        if (creature is not { IsMonster: true }) return null;

        MonsterModel disguise;
        try
        {
            disguise = Factories[rng.NextInt(Factories.Length)]();
        }
        catch
        {
            disguise = ModelDb.Monster<LeafSlimeS>();
        }

        var displayName = disguise.Title.GetFormattedText();
        NCreatureVisuals? original = null;
        NCreatureVisuals? created = null;
        var flipped = false;

        try
        {
            var node = NCombatRoom.Instance?.GetCreatureNode(creature);
            if (node?.Visuals != null)
            {
                original = node.Visuals;
                created = disguise.CreateVisuals();
                SwapVisuals(node, original, created);
                RefreshAnimator(node, disguise);
                // 通常スライムは左向き。画面左の敵（カイザーのクラッシャー等）はプレイヤーへ向くよう反転する。
                flipped = TryFaceTowardPlayer(creature, node, created);
            }
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Slime disguise visuals failed: {e.Message}");
        }

        return new State
        {
            DisplayName = displayName,
            Disguise = disguise,
            OriginalVisuals = original,
            DisguiseVisuals = created,
            FlippedForLeftSide = flipped
        };
    }

    public static void Restore(Creature creature, State? state)
    {
        if (state == null) return;

        try
        {
            var node = NCombatRoom.Instance?.GetCreatureNode(creature);
            if (node == null || state.OriginalVisuals == null) return;

            var disguise = state.DisguiseVisuals;
            if (disguise != null && GodotObject.IsInstanceValid(disguise))
            {
                SwapVisuals(node, disguise, state.OriginalVisuals);
                if (creature.Monster != null)
                    RefreshAnimator(node, creature.Monster);
                disguise.QueueFree();
            }
            else
            {
                state.OriginalVisuals.Visible = true;
                SetVisuals(node, state.OriginalVisuals);
                if (creature.Monster != null)
                    RefreshAnimator(node, creature.Monster);
            }
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Slime disguise restore failed: {e.Message}");
        }
    }

    private static void SwapVisuals(NCreature node, NCreatureVisuals from, NCreatureVisuals to)
    {
        var parent = from.GetParent();
        if (parent != null)
        {
            var index = from.GetIndex();
            if (to.GetParent() != parent)
                parent.AddChild(to);
            parent.MoveChild(to, index);
            from.Visible = false;
            to.Visible = true;
        }

        SetVisuals(node, to);
    }

    private static void SetVisuals(NCreature node, NCreatureVisuals visuals) =>
        VisualsField?.SetValue(node, visuals);

    private static void RefreshAnimator(NCreature node, MonsterModel monster)
    {
        try
        {
            var visuals = node.Visuals;
            var spine = visuals?.SpineBody;
            if (spine == null || visuals == null) return;

            visuals.SetUpSkin(monster);
            var animator = monster.GenerateAnimator(spine);
            SpineAnimatorField?.SetValue(node, animator);
            ConnectSpineMethod?.Invoke(node, null);
            node.SetAnimationTrigger("Idle");
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Slime disguise animator refresh failed: {e.Message}");
        }
    }

    /// <summary>
    /// 敵がプレイヤーより左にいるとき、デフォルト左向きのスライム見た目を右向きへ反転する。
    /// </summary>
    private static bool TryFaceTowardPlayer(Creature enemy, NCreature enemyNode, NCreatureVisuals disguise)
    {
        try
        {
            var player = enemy.CombatState?.GetOpponentsOf(enemy).FirstOrDefault();
            if (player == null) return false;

            var playerNode = NCombatRoom.Instance?.GetCreatureNode(player);
            if (playerNode == null) return false;

            // 画面左の敵 → プレイヤー（右）を向くため Scale.X を負にする
            if (enemyNode.GlobalPosition.X >= playerNode.GlobalPosition.X)
                return false;

            var scale = disguise.Scale;
            var absX = Math.Abs(scale.X);
            if (absX < 0.001f)
            {
                var def = Math.Abs(disguise.DefaultScale);
                absX = def > 0.001f ? def : 1f;
            }

            var y = Math.Abs(scale.Y) > 0.001f ? scale.Y : absX;
            disguise.Scale = new Vector2(-absX, y);
            return true;
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Slime disguise facing flip failed: {e.Message}");
            return false;
        }
    }
}
