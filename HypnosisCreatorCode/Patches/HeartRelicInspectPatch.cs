using System.Collections.Generic;
using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.InspectScreens;
using MegaCrit.Sts2.addons.mega_text;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// レリック詳細（左クリックで開く画面）でも右クリック発動できるようにする。
/// 本家の左右矢印はそのまま（コントローラー用）。
/// </summary>
public static class HeartRelicInspectPatch
{
    private static readonly AccessTools.FieldRef<NInspectRelicScreen, IReadOnlyList<RelicModel>> Relics =
        AccessTools.FieldRefAccess<NInspectRelicScreen, IReadOnlyList<RelicModel>>("_relics");

    private static readonly AccessTools.FieldRef<NInspectRelicScreen, int> Index =
        AccessTools.FieldRefAccess<NInspectRelicScreen, int>("_index");

    private static readonly AccessTools.FieldRef<NInspectRelicScreen, MegaLabel> RarityLabel =
        AccessTools.FieldRefAccess<NInspectRelicScreen, MegaLabel>("_rarityLabel");

    [HarmonyPatch(typeof(NInspectRelicScreen), "_GuiInput")]
    [HarmonyPrefix]
    public static bool GuiInputPrefix(NInspectRelicScreen __instance, InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
            return true;

        if (CombatManager.Instance is not { IsInProgress: true }) return true;

        var relics = Relics(__instance);
        var index = Index(__instance);
        if (relics == null || index < 0 || index >= relics.Count) return true;

        var model = relics[index];
        if (model is not EnemyHeartRelic heart) return true;

        if (!HeartRelicActivation.TryBeginFromModel(heart, heart.Owner)) return true;

        __instance.AcceptEvent();
        return false;
    }

    [HarmonyPatch(typeof(NInspectRelicScreen), "UpdateRelicDisplay")]
    [HarmonyPostfix]
    public static void UpdateRelicDisplayPostfix(NInspectRelicScreen __instance)
    {
        var relics = Relics(__instance);
        var index = Index(__instance);
        if (relics == null || index < 0 || index >= relics.Count) return;

        var model = relics[index];
        if (model is not EnemyHeartRelic heart) return;
        if (!HeartRelicUi.ShouldHighlightForActivation(heart, heart.Owner)) return;

        var label = RarityLabel(__instance);
        if (label == null) return;

        label.Modulate = HeartRelicUi.ActivatableModulate;
    }
}
