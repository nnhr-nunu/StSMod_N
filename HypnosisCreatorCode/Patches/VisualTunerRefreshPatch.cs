using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Config;
using MegaCrit.Sts2.Core.Nodes;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 新しいノードが出たタイミングで見た目設定を再適用する（短時間にまとめて1回）。
/// カードライブラリのポートレート追加では全再適用しない（本家一覧を壊さない）。
/// </summary>
[HarmonyPatch(typeof(NGame), nameof(NGame._Ready))]
public static class VisualTunerRefreshPatch
{
    private static bool _hooked;
    private static bool _applyQueued;

    public static void Postfix()
    {
        if (_hooked) return;
        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree == null) return;

        tree.NodeAdded += OnNodeAdded;
        _hooked = true;
        QueueApply();
    }

    private static void OnNodeAdded(Node node)
    {
        if (node is not (Sprite2D or AnimatedSprite2D or TextureRect)) return;

        // カード絵は UpdatePortrait 側で都度適用。ここで ApplyAll するとライブラリ一覧が荒れる。
        if (VisualTuner.IsCardPortraitNode(node))
            return;

        // パスが後から付くこともあるので、名前か種類で軽く絞る
        var name = node.Name.ToString();
        if (name is "Visuals" or "Background")
        {
            QueueApply();
            return;
        }

        if (node is Sprite2D sprite && sprite.Texture != null)
            QueueApply();
        else if (node is AnimatedSprite2D animated && animated.SpriteFrames != null)
            QueueApply();
        else if (node is TextureRect rect && rect.Texture != null)
            QueueApply();
    }

    private static void QueueApply()
    {
        if (_applyQueued) return;
        _applyQueued = true;
        Callable.From(FlushApply).CallDeferred();
    }

    private static void FlushApply()
    {
        _applyQueued = false;
        VisualTuner.ApplyAll();
    }
}
