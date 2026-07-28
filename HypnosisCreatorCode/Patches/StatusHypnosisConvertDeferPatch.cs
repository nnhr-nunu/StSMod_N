using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 本家 <see cref="CardPileCmd.AddToCombatAndPreview{T}"/> 完了後に状態異常催眠の置換を行う。
/// 生成直後の Transform だと苦痛の一刺し等の捨て札追加演出が消える。
/// </summary>
[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.AddToCombatAndPreview),
    typeof(Creature), typeof(PileType), typeof(int), typeof(Player), typeof(CardPilePosition))]
public static class StatusHypnosisAddToCombatAndPreviewFlushPatch
{
    public static void Postfix(ref Task __result)
    {
        var original = __result;
        __result = ContinueAsync(original);
    }

    private static async Task ContinueAsync(Task original)
    {
        await original;
        await PendingStatusHypnosisConvert.FlushIfAnyAsync();
    }
}
