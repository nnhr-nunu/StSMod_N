using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

public static class HeartCapture
{
    /// <summary>
    /// リーサル時に心臓レリックを付与。現状は汎用 StolenHeart（スタック）。
    /// 将来は Monster.Id ごとに個別レリックへ拡張する。
    /// </summary>
    public static async Task TryCapture(Player player, Creature slain)
    {
        if (!slain.IsMonster) return;

        var monsterId = slain.Monster?.Id.Entry ?? slain.ModelId.Entry;
        MainFile.Logger.Info($"Heart capture from {monsterId}");

        await RelicCmd.Obtain<StolenHeart>(player);
    }
}
