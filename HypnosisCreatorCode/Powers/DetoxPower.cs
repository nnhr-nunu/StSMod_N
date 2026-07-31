using HypnosisCreator.HypnosisCreatorCode.Extensions;
using HypnosisCreator.HypnosisCreatorCode.Rewards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// デトックス — 戦闘終了時、デッキからカードを1枚アップグレードしてもよい（スキップ可）。
/// </summary>
public class DetoxPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // No.108 カード原画をパワーアイコンに流用
    public override string CustomPackedIconPath => "detox.png".CardImagePath();
    public override string CustomBigIconPath => "detox.png".BigCardImagePath();

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        var player = Owner?.Player;
        if (player == null) return;

        var rewardCount = Math.Max(1, Amount);
        for (var i = 0; i < rewardCount; i++)
            room.AddExtraReward(player, new DetoxDeckUpgradeReward(player));

        await PowerCmd.Remove(this);
    }
}
