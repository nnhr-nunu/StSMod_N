using System.Reflection;
using Godot;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>認知シャッフルパワー行アイコンの再読込（ホバーは都度取得、行表示は NPower.Reload が必要）。</summary>
public static class CognitiveShufflePowerUi
{
    private static readonly FieldInfo? ModelField = typeof(NPower).GetField(
        "_model",
        BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo? ReloadMethod = typeof(NPower).GetMethod(
        "Reload",
        BindingFlags.Instance | BindingFlags.NonPublic);

    public static void RefreshRowIcon(PowerModel power)
    {
        if (power.Owner == null || ReloadMethod == null) return;

        try
        {
            var root = NCombatRoom.Instance?.GetCreatureNode(power.Owner);
            if (root == null) return;
            RefreshRecursive(root, power);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Cognitive shuffle power icon refresh failed: {e.Message}");
        }
    }

    private static void RefreshRecursive(Node node, PowerModel power)
    {
        if (node is NPower np && ReferenceEquals(ModelField?.GetValue(np), power))
            ReloadMethod?.Invoke(np, null);

        foreach (var child in node.GetChildren())
            RefreshRecursive(child, power);
    }
}
