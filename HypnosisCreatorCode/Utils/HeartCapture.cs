using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

public static class HeartCapture
{
    /// <summary>リーサル時に敵固有心臓を付与。未登録モンスターは StolenHeart にフォールバック。</summary>
    public static async Task TryCapture(Player player, Creature slain)
    {
        if (!slain.IsMonster) return;

        var monsterId = slain.Monster?.Id.Entry ?? slain.ModelId.Entry;
        MainFile.Logger.Info($"Heart capture from {monsterId}");

        var heartType = HeartRegistry.ResolveHeartType(monsterId);
        if (heartType != null)
        {
            await ObtainHeart(player, heartType);
            return;
        }

        await RelicCmd.Obtain<StolenHeart>(player);
    }

    private static async Task ObtainHeart(Player player, Type heartType)
    {
        var method = typeof(RelicCmd).GetMethods()
            .First(m => m.Name == nameof(RelicCmd.Obtain) && m.IsGenericMethodDefinition && m.GetParameters().Length == 1);
        var task = (Task)method.MakeGenericMethod(heartType).Invoke(null, [player])!;
        await task;
    }
}
