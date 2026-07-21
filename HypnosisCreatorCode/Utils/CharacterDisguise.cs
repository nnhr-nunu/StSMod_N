using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 認知シャッフル用。プレイヤー見た目・攻撃アニメを他キャラへ一時差し替え（名前は変えない）。
/// </summary>
public static class CharacterDisguise
{
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
        public required CharacterModel DisguiseCharacter { get; init; }
        public NCreatureVisuals? OriginalVisuals { get; init; }
        public NCreatureVisuals? DisguiseVisuals { get; init; }
    }

    public static State? Apply(Creature creature, CharacterModel disguiseCharacter)
    {
        if (creature is not { IsPlayer: true }) return null;

        NCreatureVisuals? original = null;
        NCreatureVisuals? created = null;

        try
        {
            var node = NCombatRoom.Instance?.GetCreatureNode(creature);
            if (node?.Visuals != null)
            {
                original = node.Visuals;
                created = disguiseCharacter.CreateVisuals();
                SwapVisuals(node, original, created);
                RefreshAnimator(node, disguiseCharacter);
            }
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Character disguise visuals failed: {e.Message}");
        }

        return new State
        {
            DisguiseCharacter = disguiseCharacter,
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
            var originalCharacter = creature.Player?.Character;

            if (disguise != null && GodotObject.IsInstanceValid(disguise))
            {
                SwapVisuals(node, disguise, state.OriginalVisuals);
                if (originalCharacter != null)
                    RefreshAnimator(node, originalCharacter);
                disguise.QueueFree();
            }
            else
            {
                state.OriginalVisuals.Visible = true;
                SetVisuals(node, state.OriginalVisuals);
                if (originalCharacter != null)
                    RefreshAnimator(node, originalCharacter);
            }
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Character disguise restore failed: {e.Message}");
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

    private static void RefreshAnimator(NCreature node, CharacterModel character)
    {
        try
        {
            var visuals = node.Visuals;
            var spine = visuals?.SpineBody;
            if (spine == null || visuals == null)
            {
                // Spine 無し（自キャラ連番など）へ戻すときも Idle を叩いてフレーム側に任せる
                node.SetAnimationTrigger("Idle");
                return;
            }

            var animator = character.GenerateAnimator(spine);
            SpineAnimatorField?.SetValue(node, animator);
            ConnectSpineMethod?.Invoke(node, null);
            node.SetAnimationTrigger("Idle");
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Character disguise animator refresh failed: {e.Message}");
        }
    }
}
