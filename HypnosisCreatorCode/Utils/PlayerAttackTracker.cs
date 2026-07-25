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

    public static void BeginPlayerTurn(Player player) =>
        Field.Get(player).BeginPlayerTurn();

    /// <summary>プレイヤーターン終了直前。アタック未プレイなら未攻撃ターン数を +1。</summary>
    public static void FinalizePlayerTurn(Player player) =>
        Field.Get(player).FinalizePlayerTurn();

    public static void RecordAttack(Player player, int turn) =>
        Field.Get(player).RecordAttack(turn);

    public static bool AttackedOnTurn(Player player, int turn) =>
        Field.Get(player).AttackedOnTurn(turn);

    /// <summary>
    /// 終了済みプレイヤーターンのうち、アタックを1枚もプレイしていないターン数。
    /// 急所の一刺し: 「この戦闘中、アタックをプレイしていないターン1につき」。
    /// </summary>
    public static int CompletedNonAttackTurns(Player player) =>
        Field.Get(player).NonAttackCompletedTurns;
}

public sealed class AttackState
{
    private readonly HashSet<int> _turnsWithAttack = new();

    public int NonAttackCompletedTurns { get; private set; }

    public bool AttackedThisTurn { get; private set; }

    public void Reset()
    {
        _turnsWithAttack.Clear();
        NonAttackCompletedTurns = 0;
        AttackedThisTurn = false;
    }

    public void BeginPlayerTurn() => AttackedThisTurn = false;

    public void FinalizePlayerTurn()
    {
        if (!AttackedThisTurn)
            NonAttackCompletedTurns++;
    }

    public void RecordAttack(int turn)
    {
        _turnsWithAttack.Add(turn);
        AttackedThisTurn = true;
    }

    public bool AttackedOnTurn(int turn) => _turnsWithAttack.Contains(turn);
}
