using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// マルチのターン終了ピン押下時、ヒプノクリエイターなら指パッチン SE を鳴らす。
/// 本家の送信クールダウンとは独立（連打可）。セリフは characters.json の endTurnPing。
/// </summary>
[HarmonyPatch(typeof(FlavorSynchronizer), nameof(FlavorSynchronizer.SendEndTurnPing))]
public static class EndTurnPingFingerSnapPatch
{
    private static readonly PropertyInfo? LocalPlayerProperty =
        AccessTools.Property(typeof(FlavorSynchronizer), "LocalPlayer");

    public static void Prefix(FlavorSynchronizer __instance)
    {
        if (LocalPlayerProperty == null) return;

        var localPlayer = LocalPlayerProperty.GetValue(__instance) as Player;
        if (!FingerSnapCardRules.IsHypnosisCreatorPlayer(localPlayer)) return;

        FingerSnapSfx.PlayNormal();
    }
}
