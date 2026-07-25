using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// プレイヤーがどのターンにアタックカードを敵へプレイしたかを記録する。
/// ラポール・急所の一刺し等、複数カードで共有する。
/// </summary>
public static class PlayerAttackTracker
{
    private static readonly NotNullSpireField<Player, AttackState> Field = new(() => new AttackState());

    public static void Reset(Player player) => Field.Get(player).Reset();

    public static void RecordAttack(Player player, int turn) =>
        Field.Get(player).RecordAttack(turn);

    public static bool AttackedOnTurn(Player player, int turn) =>
        Field.Get(player).AttackedOnTurn(turn);

    /// <summary>
    /// 終了済みターンのうち、アタックを1枚もプレイしていないターン数（現在ターンは含めない）。
    /// 急所の一刺し: 「この戦闘中、アタックをプレイしていないターン1につき」。
    /// </summary>
    public static int CompletedNonAttackTurns(Player player, int currentTurn) =>
        Field.Get(player).CountCompletedNonAttackTurns(currentTurn);
}

public sealed class AttackState
{
    private readonly HashSet<int> _turnsWithAttack = new();

    public void Reset() => _turnsWithAttack.Clear();

    public void RecordAttack(int turn) => _turnsWithAttack.Add(turn);

    public bool AttackedOnTurn(int turn) => _turnsWithAttack.Contains(turn);

    public int CountCompletedNonAttackTurns(int currentTurn)
    {
        if (currentTurn <= 1) return 0;

        var count = 0;
        for (var t = 1; t < currentTurn; t++)
        {
            if (!_turnsWithAttack.Contains(t))
                count++;
        }

        return count;
    }
}
