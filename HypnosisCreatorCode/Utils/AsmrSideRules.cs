using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Random;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>ASMR催眠 — プレイヤーの左右担当割り当てと、敵への最終攻撃側マーカー。</summary>
public static class AsmrSideRules
{
    private static Dictionary<Player, bool>? _playerSides;

    public static bool IsAssigned => _playerSides != null;

    public static void Reset() => _playerSides = null;

    public static bool? TryGetPlayerSide(Player player) =>
        _playerSides != null && _playerSides.TryGetValue(player, out var isLeft) ? isLeft : null;

    public static async Task AssignPlayersAsync(
        PlayerChoiceContext choiceContext,
        IReadOnlyList<Player> players,
        Rng rng,
        Creature? applier)
    {
        if (players.Count <= 1 || _playerSides != null) return;

        _playerSides = new Dictionary<Player, bool>();
        var leftCount = players.Count / 2;
        var order = Enumerable.Range(0, players.Count).ToList();
        for (var i = order.Count - 1; i > 0; i--)
        {
            var j = rng.NextInt(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        for (var i = 0; i < players.Count; i++)
        {
            var player = players[order[i]];
            var isLeft = i < leftCount;
            _playerSides[player] = isLeft;
            await SyncAssignmentMarkerAsync(choiceContext, player.Creature, isLeft, applier);
        }
    }

    public static bool ResolvePlaySide(Player player, bool? lastWasLeft) =>
        TryGetPlayerSide(player) ?? !(lastWasLeft ?? false);

    public static async Task SyncEnemyHitSideAsync(
        PlayerChoiceContext choiceContext,
        Creature enemy,
        bool isLeft,
        Creature? applier)
    {
        if (!enemy.IsEnemy) return;

        if (isLeft)
        {
            var right = enemy.GetPower<AsmrRightPower>();
            if (right != null) await PowerCmd.Remove(right);
            if (enemy.GetPower<AsmrLeftPower>() == null)
                await PowerCmd.Apply<AsmrLeftPower>(choiceContext, enemy, 1M, applier, null!, silent: true);
        }
        else
        {
            var left = enemy.GetPower<AsmrLeftPower>();
            if (left != null) await PowerCmd.Remove(left);
            if (enemy.GetPower<AsmrRightPower>() == null)
                await PowerCmd.Apply<AsmrRightPower>(choiceContext, enemy, 1M, applier, null!, silent: true);
        }
    }

    private static async Task SyncAssignmentMarkerAsync(
        PlayerChoiceContext choiceContext,
        Creature playerCreature,
        bool isLeft,
        Creature? applier)
    {
        if (!playerCreature.IsPlayer) return;

        if (isLeft)
        {
            var right = playerCreature.GetPower<AsmrRightPower>();
            if (right != null) await PowerCmd.Remove(right);
            if (playerCreature.GetPower<AsmrLeftPower>() == null)
                await PowerCmd.Apply<AsmrLeftPower>(choiceContext, playerCreature, 1M, applier, null!, silent: true);
        }
        else
        {
            var left = playerCreature.GetPower<AsmrLeftPower>();
            if (left != null) await PowerCmd.Remove(left);
            if (playerCreature.GetPower<AsmrRightPower>() == null)
                await PowerCmd.Apply<AsmrRightPower>(choiceContext, playerCreature, 1M, applier, null!, silent: true);
        }
    }
}
