using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// loc テーブルから SmartFormat を通さず生文字列を取得する（キーワード判定用）。
/// </summary>
internal static class CardLocRawText
{
    private static readonly FieldInfo? TranslationsField =
        AccessTools.Field(typeof(LocTable), "_translations");

    public static string? TryGet(string tableName, string key)
    {
        try
        {
            var table = LocManager.Instance?.GetTable(tableName);
            if (table == null) return null;
            return TryGetFromTable(table, key, out var text) ? text : null;
        }
        catch
        {
            return null;
        }
    }

    public static bool TryGetFromTable(LocTable table, string key, out string text)
    {
        text = "";
        if (TranslationsField == null) return false;

        var dict = TranslationsField.GetValue(table) as Dictionary<string, string>;
        if (dict == null || !dict.TryGetValue(key, out var raw)) return false;

        text = raw;
        return true;
    }
}
