using MegaCrit.Sts2.Core.Entities.Players;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 敵攻撃意図の読取り中だけ <see cref="MegaCrit.Sts2.Core.Context.LocalContext.GetMe"/> を差し替える。
/// マルチで各端末の「自分」が違うと AttackIntent.GetSingleDamage がズレるため、カード所有者視点に固定する。
/// </summary>
public static class EnemyAttackIntentPerspective
{
    private static readonly Stack<Player> Overrides = new();

    public static bool TryPeek(out Player player)
    {
        if (Overrides.Count > 0)
        {
            player = Overrides.Peek();
            return true;
        }

        player = null!;
        return false;
    }

    public static IDisposable Begin(Player perspectivePlayer)
    {
        ArgumentNullException.ThrowIfNull(perspectivePlayer);
        Overrides.Push(perspectivePlayer);
        return new Scope();
    }

    private sealed class Scope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            if (Overrides.Count > 0)
                Overrides.Pop();
        }
    }
}
