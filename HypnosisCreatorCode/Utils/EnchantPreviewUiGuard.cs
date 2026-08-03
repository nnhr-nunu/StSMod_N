using Godot;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// エンチャント付与確認（<see cref="NEnchantPreview"/>）表示中は戦闘フック付きダメージプレビューを抑止する。
/// </summary>
internal static class EnchantPreviewUiGuard
{
    private static int _depth;

    public static bool IsActive => _depth > 0;

    public static bool IsUnderEnchantPreview(Node? node)
    {
        if (IsActive) return true;

        for (var p = node; p != null; p = p.GetParent())
        {
            if (p is NEnchantPreview)
                return true;
        }

        return false;
    }

    public static void Push() => _depth++;

    public static void Pop()
    {
        if (_depth > 0)
            _depth--;
    }
}
