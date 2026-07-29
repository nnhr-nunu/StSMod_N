using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// カードライブラリ表示中は mod の説明・キーワード・見た目改変をスキップし本家と同じにする。
/// <see cref="Patches.CardLibraryUiScopePatch"/> が <c>NCard.UpdateVisuals</c> 中だけ depth を上げる。
/// </summary>
internal static class CardLibraryUiGuard
{
    private static int _depth;

    public static bool IsActive => _depth > 0;

    public static bool IsUnderCardLibrary(Node? node)
    {
        for (var p = node; p != null; p = p.GetParent())
        {
            if (p is NCardLibrary or NCardLibraryGrid)
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
