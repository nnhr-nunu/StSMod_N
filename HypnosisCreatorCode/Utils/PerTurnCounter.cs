namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>1ターン内のプレイ回数などをカウントする小さなユーティリティ（時止めストライク等）。</summary>
public sealed class PerTurnCounter
{
    private int _turn = -1;
    private int _count;

    /// <summary>現在ターンでのカウントを1増やして返す（ターンが変わっていれば0から数え直す）。</summary>
    public int Increment(int currentTurn)
    {
        if (_turn != currentTurn)
        {
            _turn = currentTurn;
            _count = 0;
        }

        return ++_count;
    }

    public int Get(int currentTurn) => _turn == currentTurn ? _count : 0;
}
