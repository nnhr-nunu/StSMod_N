using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards.Common;
using HypnosisCreator.HypnosisCreatorCode.Cards.Rare;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 複製は playCount に +1 するだけなので、リプレイ付きカードは「基礎＋リプレイ」ブロックが1回足りなくなる。
/// 複製発動時、確定済みの BaseReplayCount ぶんを追加する（スパンキング／糸色丁頁）。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyCardPlayCount))]
public static class DuplicationExtraReplayPatch
{
    public static void Postfix(CardModel card, ref int __result, List<AbstractModel> modifyingModels)
    {
        if (modifyingModels.Count == 0) return;
        if (!modifyingModels.Any(m => m is DuplicationPower)) return;
        if (card.BaseReplayCount <= 0) return;

        __result += card switch
        {
            Spanking s => s.DynamicVars["Replays"].IntValue,
            InfiniteUpgradeString i => i.DynamicVars["Replays"].IntValue,
            _ => 0
        };
    }
}
