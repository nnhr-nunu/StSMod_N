using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Config;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// キャラ選択で背景シーンが載った直後／設定サブメニュー閉鎖後に 1枚絵チューニングを再適用する。
/// </summary>
[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))]
public static class CharacterSelectBgTunerOnSelectPatch
{
    public static void Postfix() => CharacterSelectBgTunerPatch.QueueSelectBgApply();
}

[HarmonyPatch(typeof(NCharacterSelectScreen), "OnSubmenuClosed")]
public static class CharacterSelectBgTunerOnSubmenuClosedPatch
{
    public static void Postfix() => CharacterSelectBgTunerPatch.QueueSelectBgApply();
}

internal static class CharacterSelectBgTunerPatch
{
    private static bool _queued;

    public static void QueueSelectBgApply()
    {
        if (_queued) return;
        _queued = true;
        Callable.From(Flush).CallDeferred();
    }

    private static void Flush()
    {
        _queued = false;
        VisualTuner.ApplySelectBackground();
    }
}
