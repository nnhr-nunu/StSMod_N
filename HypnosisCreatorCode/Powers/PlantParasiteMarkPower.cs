using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 植物寄生催眠 — 付与された戦闘の終了時に心臓を入手する（キル不要）。
/// </summary>
public class PlantParasiteMarkPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var player = Applier?.Player ?? Owner?.CombatState?.Players.FirstOrDefault();
        if (player == null || Owner == null) return;
        await HeartCapture.TryCapture(player, Owner);
    }
}
