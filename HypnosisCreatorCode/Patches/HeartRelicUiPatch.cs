using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 希少な心臓の金色ハイライトと右クリック説明ホバー。
/// </summary>
[HarmonyPatch]
public static class HeartRelicUiPatch
{
    [HarmonyPatch(typeof(NRelicInventoryHolder), "RefreshStatus")]
    [HarmonyPostfix]
    public static void RefreshStatusPostfix(NRelicInventoryHolder __instance)
    {
        var relic = __instance.Relic;
        if (relic == null) return;

        var player = HeartRelicActivation.ResolvePlayerFromHolder(__instance);
        var heart = HeartRelicActivation.ResolveOwnedHeart(relic.Model, player);
        HeartRelicUi.ApplyHolderVisual(heart, relic.Icon, player);
    }

    [HarmonyPatch(typeof(RelicModel), nameof(RelicModel.HoverTips), MethodType.Getter)]
    [HarmonyPostfix]
    public static void HoverTipsPostfix(RelicModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        if (__instance is not EnemyHeartRelic heart) return;
        if (!HeartRelicUi.ShouldShowActivationHover(heart, heart.Owner)) return;

        var tips = __result.ToList();
        tips.Add(HeartRelicUi.CreateActivationHoverTip());
        __result = tips;
    }
}
