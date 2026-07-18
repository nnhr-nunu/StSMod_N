using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 「引き寄せ（Pull）」の暫定実装。対象ごとに1度だけ「引き寄せ済み」フラグを立てる。
/// 本来の意図（手元に引き寄せる演出）は未確定のため、状態フラグのみで近似する。
/// </summary>
public static class PullTracker
{
    private static readonly NotNullSpireField<Creature, PullState> Field = new(() => new PullState());

    public static bool IsPulled(Creature creature) => Field.Get(creature).Pulled;

    /// <summary>まだ引き寄せていなければ引き寄せて true、既に引き寄せ済みなら false を返す。</summary>
    public static bool TryPull(Creature creature)
    {
        var state = Field.Get(creature);
        if (state.Pulled) return false;
        state.Pulled = true;
        return true;
    }
}

public sealed class PullState
{
    public bool Pulled { get; set; }
}
