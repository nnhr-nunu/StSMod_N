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

        try
        {
            var node = NCombatRoom.Instance?.GetCreatureNode(creature);
            if (node?.Visuals != null)
            {
                original = node.Visuals;
                created = disguise.CreateVisuals();
                SwapVisuals(node, original, created);
                RefreshAnimator(node, disguise);
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
            DisguiseVisuals = created
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
}
