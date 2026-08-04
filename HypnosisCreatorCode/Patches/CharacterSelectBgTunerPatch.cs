using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Config;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// キャラ選択で背景シーンが載った直後／設定サブメニュー閉鎖後に 1枚絵チューニングを再適用する。
/// マルチロビーは RemotePlayerContainer 表示後にレイアウトが変わるため、数フレーム後にも再適用する。
/// </summary>
[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))]
public static class CharacterSelectBgTunerOnSelectPatch
{
    public static void Postfix() => CharacterSelectBgTunerPatch.QueueSelectBgApply();
}

[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.OnSubmenuOpened))]
public static class CharacterSelectBgTunerOnSubmenuOpenedPatch
{
    public static void Postfix() => CharacterSelectBgTunerPatch.QueueSelectBgApplyAfterLayout();
}

[HarmonyPatch(typeof(NCharacterSelectScreen), "OnLocalCharacterChangedForRandom")]
public static class CharacterSelectBgTunerOnRandomPatch
{
    public static void Postfix() => CharacterSelectBgTunerPatch.QueueSelectBgApply();
}

[HarmonyPatch(typeof(NCharacterSelectScreen), "OnSubmenuClosed")]
public static class CharacterSelectBgTunerOnSubmenuClosedPatch
{
    public static void Postfix() => CharacterSelectBgTunerPatch.QueueSelectBgApply();
}

[HarmonyPatch(typeof(NCharacterSelectScreen), "OnEmbarkPressed")]
public static class CharacterSelectBgTunerOnEmbarkPatch
{
    public static void Postfix() => CharacterSelectBgTunerPatch.QueueSelectBgApplyAfterLayout();
}

[HarmonyPatch(typeof(NCharacterSelectScreen), "OnUnreadyPressed")]
public static class CharacterSelectBgTunerOnUnreadyPatch
{
    public static void Postfix() => CharacterSelectBgTunerPatch.QueueSelectBgApplyAfterLayout();
}

internal static class CharacterSelectBgTunerPatch
{
    private const int LayoutSettleFrames = 2;

    private static bool _queued;
    private static bool _layoutWaitActive;
    private static int _layoutWaitFrames;
    private static SceneTree? _layoutTree;

    public static void QueueSelectBgApply()
    {
        if (_queued)
            return;

        _queued = true;
        Callable.From(FlushImmediate).CallDeferred();
    }

    public static void QueueSelectBgApplyAfterLayout(int frames = LayoutSettleFrames)
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree == null)
        {
            QueueSelectBgApply();
            return;
        }

        _layoutWaitFrames = Math.Max(_layoutWaitFrames, frames);
        if (_layoutWaitActive)
            return;

        _layoutWaitActive = true;
        _layoutTree = tree;
        tree.ProcessFrame += OnLayoutWaitFrame;
    }

    private static void OnLayoutWaitFrame()
    {
        _layoutWaitFrames--;
        if (_layoutWaitFrames > 0)
            return;

        if (_layoutTree != null)
            _layoutTree.ProcessFrame -= OnLayoutWaitFrame;

        _layoutWaitActive = false;
        _layoutTree = null;
        _layoutWaitFrames = 0;
        VisualTuner.ApplySelectBackground();
    }

    private static void FlushImmediate()
    {
        _queued = false;
        VisualTuner.ApplySelectBackground();
        QueueSelectBgApplyAfterLayout();
    }
}
