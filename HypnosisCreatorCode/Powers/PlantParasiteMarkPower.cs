using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 心臓寄生催眠 — 付与時に敵 ID を予約し、戦闘終了時に追加レリック報酬として心臓を載せる（キル不要）。
/// </summary>
public class PlantParasiteMarkPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public string? CapturedMonsterId { get; private set; }
    private bool _queued;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner == null) return Task.CompletedTask;

        CapturedMonsterId = Owner.Monster?.Id.Entry ?? Owner.ModelId.Entry;
        TryQueue(applier?.Player);
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        // 本体の解決は HeartCaptureCombatEndPatch で一括 Flush。
        // AfterApplied が飛ばなかった場合の保険として、ここで ID を拾い直す。
        if (string.IsNullOrWhiteSpace(CapturedMonsterId) && Owner != null)
            CapturedMonsterId = Owner.Monster?.Id.Entry ?? Owner.ModelId.Entry;

        TryQueue(Applier?.Player ?? Owner?.CombatState?.Players.FirstOrDefault());
        return Task.CompletedTask;
    }

    private void TryQueue(MegaCrit.Sts2.Core.Entities.Players.Player? player)
    {
        if (_queued || player == null || string.IsNullOrWhiteSpace(CapturedMonsterId)) return;
        HeartCapture.QueueCapture(player, CapturedMonsterId);
        _queued = true;
    }
}
