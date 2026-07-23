using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// カード本文の {Doom:diff()} に、照準中の沼倍率を反映する（緑数字）。
/// </summary>
[HarmonyPatch(typeof(DynamicVar), nameof(DynamicVar.UpdateCardPreview))]
public static class DoomDynamicVarPreviewPatch
{
    public static void Postfix(
        DynamicVar __instance,
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        _ = previewMode;
        _ = runGlobalHooks;
        if (__instance.Name != "Doom") return;

        var previewTarget = target ?? card.CurrentTarget;
        if (previewTarget is not { IsAlive: true, IsEnemy: true }) return;

        var raw = __instance.BaseValue;
        var scaled = FetishCombat.ScaleDoomByBog(previewTarget, (int)raw);
        if (scaled == raw) return;

        CardDamagePreview.SetPreviewPair(__instance, raw, scaled);
    }
}
