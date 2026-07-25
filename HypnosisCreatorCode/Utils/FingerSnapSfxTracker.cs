namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 多段攻撃中の指パッチン SE 用。AttackCommand.Execute のスコープでヒット番号を数える。
/// </summary>
internal static class FingerSnapSfxTracker
{
    private sealed class Scope
    {
        public int TotalHits = 1;
        public int CurrentHit;
    }

    private static readonly AsyncLocal<Stack<Scope>?> Stack = new();

    public static void BeginAttack(int totalHits)
    {
        var stack = Stack.Value;
        if (stack == null)
        {
            stack = new Stack<Scope>();
            Stack.Value = stack;
        }

        stack.Push(new Scope { TotalHits = Math.Max(1, totalHits) });
    }

    public static void EndAttack()
    {
        var stack = Stack.Value;
        if (stack == null || stack.Count == 0) return;
        stack.Pop();
        if (stack.Count == 0)
            Stack.Value = null;
    }

    /// <summary>次の1ヒット分を進め、総ヒット数と通算番号を返す。</summary>
    public static bool TryAdvance(out int totalHits, out int hitIndex)
    {
        totalHits = 1;
        hitIndex = 1;

        var stack = Stack.Value;
        if (stack == null || stack.Count == 0) return false;

        var scope = stack.Peek();
        scope.CurrentHit++;
        totalHits = scope.TotalHits;
        hitIndex = scope.CurrentHit;
        return true;
    }
}
