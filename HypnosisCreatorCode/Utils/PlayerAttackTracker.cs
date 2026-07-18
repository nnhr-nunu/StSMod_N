using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// プレイヤーが直近どのターンにアタックカードを敵へプレイしたかを記録する。
/// ラポール・急所の一刺し等、複数カードで共有する。
/// </summary>
public static class PlayerAttackTracker
{
    private static readonly NotNullSpireField<Player, AttackState> Field = new(() => new AttackState());

    public static void RecordAttack(Player player, int turn)
    {
        var state = Field.Get(player);
        if (turn > state.LastAttackTurn) state.LastAttackTurn = turn;
    }

    public static bool AttackedOnTurn(Player player, int turn) => Field.Get(player).LastAttackTurn == turn;

    /// <summary>現在ターンから最後にアタックした差分（未攻撃なら現在ターン数を返す）。</summary>
    public static int TurnsSinceLastAttack(Player player, int currentTurn)
    {
        var last = Field.Get(player).LastAttackTurn;
        return last < 0 ? currentTurn : Math.Max(0, currentTurn - last);
    }
}

public sealed class AttackState
{
    public int LastAttackTurn { get; set; } = -1;
}
