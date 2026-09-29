namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 表示用キーワードのツールチップ（<see cref="Patches.MechanicKeywordPatch"/>）の再入を検出する。
/// Loc 解決やツールチップ見積もりが同じ処理を再び呼ぶとスタック溢れになるため。
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
