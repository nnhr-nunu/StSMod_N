using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// カタレプシー（UG）— 対象がトランス中なら、本家 <see cref="SlowPower"/> のターン開始リセットを打ち消す。
/// トランスが一度でも0になったら蓄積バンクはリセット（再付与だけでは復元しない）。
/// アイコンは本家スロー（カタツムリ系）を流用する。
/// </summary>
public class CatalepsyPower : HypnosisCreatorPower
{
    private static readonly PowerModel VanillaSlow = ModelDb.Power<SlowPower>();

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override string CustomPackedIconPath => VanillaSlow.PackedIconPath;
    public override string CustomBigIconPath => VanillaSlow.ResolvedBigIconPath;

    private decimal _savedSlowAmount;
    private bool _tranceBrokenSinceLastSave;

    internal static void NotifyTranceBroken(Creature? owner) =>
        owner?.GetPower<CatalepsyPower>()?.OnTranceBroken();

    private void OnTranceBroken()
    {
        _savedSlowAmount = 0M;
        _tranceBrokenSinceLastSave = true;
    }

    public override Task AfterSideTurnEnd(
        MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || side != CombatSide.Player) return Task.CompletedTask;

        if (!TranceCombat.HasTrance(Owner))
        {
            _savedSlowAmount = 0M;
            return Task.CompletedTask;
        }

        // トランスが途切れたあと同ターンで再付与された場合、古いスロー量を保存しない
        if (_tranceBrokenSinceLastSave) return Task.CompletedTask;

        var slow = Owner.GetPower<SlowPower>();
        if (slow != null)
            _savedSlowAmount = slow.DynamicVars["SlowAmount"].BaseValue;
        return Task.CompletedTask;
    }

    /// <summary>
    /// 本家 <see cref="SlowPower"/> は <see cref="AbstractModel.AfterSideTurnStart"/> で蓄積を0に戻す。
    /// 復元は必ず <see cref="AbstractModel.AfterSideTurnStartLate"/> で行う（メトロノームと同型）。
    /// </summary>
    public override Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Owner == null || !participants.Contains(Owner)) return Task.CompletedTask;

        if (!TranceCombat.HasTrance(Owner))
        {
            _savedSlowAmount = 0M;
            _tranceBrokenSinceLastSave = false;
            return Task.CompletedTask;
        }

        if (_tranceBrokenSinceLastSave)
        {
            _savedSlowAmount = 0M;
            _tranceBrokenSinceLastSave = false;
            return Task.CompletedTask;
        }

        if (_savedSlowAmount <= 0M) return Task.CompletedTask;

        var slow = Owner.GetPower<SlowPower>();
        if (slow == null) return Task.CompletedTask;

        // SlowAmount を積み直す（表示は SlowPower.DisplayAmount = SlowAmount×10）
        slow.DynamicVars["SlowAmount"].BaseValue = _savedSlowAmount;
        return Task.CompletedTask;
    }
}
