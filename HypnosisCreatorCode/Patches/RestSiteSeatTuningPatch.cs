using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 篝火ルームの全席配置・FlipX 完了後（フレーム末）に席番号・マルチ人数で立ち絵を調整する。
/// </summary>
[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
public static class RestSiteSeatTuningPatch
{
    public static void Postfix(NRestSiteCharacter __instance) =>
        Callable.From(() => RestSiteSeatTuning.Apply(__instance)).CallDeferred();
}
