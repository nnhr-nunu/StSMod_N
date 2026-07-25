namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// <see cref="MegaCrit.Sts2.Core.Models.CardModel.GetKeywordsWithSources"/> パッチの再入を検出する。
/// Loc 解決がキーワード取得を再び呼ぶとデッドロック／スタック溢れになるため。
/// </summary>
internal static class KeywordPatchGuard
{
    [ThreadStatic] private static int _depth;

    public static bool IsNested => _depth > 1;

    public static void Enter() => _depth++;

    public static void Leave()
    {
        if (_depth > 0)
            _depth--;
    }
}
