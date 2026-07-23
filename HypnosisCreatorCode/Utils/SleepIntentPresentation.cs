using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 強制睡眠中は UI だけ睡眠意図を表示する。NextMove は触らず PerformMove スキップ経路を維持する。
/// </summary>
public static class SleepIntentPresentation
{
    private static readonly AbstractIntent[] SleepOnly = [new SleepIntent()];

    public static bool ShouldOverride(Creature? creature) =>
        creature is { IsAlive: true, IsEnemy: true }
        && creature.HasPower<AsleepPower>()
        && creature.GetPower<ForcedSleepVisualPower>() is { ShouldSkipPerform: true };

    public static Task UpdateIntent(NCreature node, IEnumerable<Creature> targets) =>
        ApplyIntents(node, targets, SleepOnly);

    public static async Task TryRefreshAsync(Creature? creature)
    {
        if (creature == null) return;

        try
        {
            var node = NCombatRoom.Instance?.GetCreatureNode(creature);
            if (node != null)
                await node.RefreshIntents();
        }
        catch
        {
            // Intent UI 差異時は無視
        }
    }

    private static Task ApplyIntents(
        NCreature node,
        IEnumerable<Creature> targets,
        IReadOnlyList<AbstractIntent> intents)
    {
        var container = node.IntentContainer;
        var i = 0;
        for (; i < intents.Count && i < container.GetChildCount(); i++)
        {
            var child = container.GetChild<NIntent>(i);
            child.SetFrozen(isFrozen: false);
            child.UpdateIntent(intents[i], targets, node.Entity);
        }

        var offset = (float)node.GetHashCode() * 0.01f;
        for (; i < intents.Count; i++)
        {
            var intentNode = NIntent.Create(offset + i * 0.3f);
            container.AddChildSafely(intentNode);
            intentNode.UpdateIntent(intents[i], targets, node.Entity);
        }

        var extra = container.GetChildren().TakeLast(container.GetChildCount() - i).ToList();
        foreach (var child in extra)
        {
            container.RemoveChildSafely(child);
            child.QueueFreeSafely();
        }

        return Task.CompletedTask;
    }
}
