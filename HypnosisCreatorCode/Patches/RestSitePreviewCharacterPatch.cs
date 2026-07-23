using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>シミュレータ用ダミーは Player 無しで _Ready すると落ちるため、見た目だけ残す。</summary>
[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
public static class RestSitePreviewCharacterPatch
{
    public static bool Prefix(NRestSiteCharacter __instance)
    {
        if (!__instance.HasMeta(RestSiteLayoutSimulator.PreviewMeta))
            return true;

        // 本家キャラはシーン既定のスケール・構造のまま。_Ready だけスキップ（Player 無しで落ちるため）。
        return false;
    }
}
