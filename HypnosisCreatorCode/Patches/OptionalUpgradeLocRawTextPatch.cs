using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Localization;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// BaseLib は UG カード表示時に <c>.upgradeReplaceFrom</c> / <c>.upgradeAppend</c> を常に探す。
/// 数値のみ UG のカードはキー未定義で WARN が出るため、本 mod の任意キーは未登録なら空文字を返す。
/// </summary>
[HarmonyPatch(typeof(LocTable), nameof(LocTable.GetRawText))]
internal static class OptionalUpgradeLocRawTextPatch
{
  private static bool IsOptionalUpgradeKey(string key) =>
      key.StartsWith("HYPNOSISCREATOR-", StringComparison.Ordinal)
      && (key.EndsWith(".upgradeReplaceFrom", StringComparison.Ordinal)
          || key.EndsWith(".upgradeAppend", StringComparison.Ordinal));

  private static bool Prefix(LocTable __instance, string key, ref string __result)
  {
    if (!IsOptionalUpgradeKey(key))
      return true;

    if (CardLocRawText.TryGetFromTable(__instance, key, out var text))
    {
      __result = text;
      return false;
    }

    __result = "";
    return false;
  }
}
