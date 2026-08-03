using System.Reflection;
using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// マルチのターン終了ピン押下時、ヒプノクリエイターなら指パッチン SE を鳴らす。
/// セリフは characters.json の endTurnPing を表示（本家 FlavorSynchronizer）。
/// </summary>
[HarmonyPatch(typeof(FlavorSynchronizer), nameof(FlavorSynchronizer.SendEndTurnPing))]
public static class EndTurnPingFingerSnapPatch
{
    private static readonly FieldInfo NextAllowedPingTimeField =
        AccessTools.Field(typeof(FlavorSynchronizer), "_nextAllowedPingTime");

    private static readonly PropertyInfo? LocalPlayerProperty =
        AccessTools.Property(typeof(FlavorSynchronizer), "LocalPlayer");

    public static void Prefix(FlavorSynchronizer __instance, out bool willPing)
    {
        var nextAllowed = (long)NextAllowedPingTimeField.GetValue(__instance)!;
        willPing = (long)Time.GetTicksMsec() >= nextAllowed;
    }

    public static void Postfix(FlavorSynchronizer __instance, bool willPing)
    {
        if (!willPing || LocalPlayerProperty == null) return;

        var localPlayer = LocalPlayerProperty.GetValue(__instance) as Player;
        if (!FingerSnapCardRules.IsHypnosisCreatorPlayer(localPlayer)) return;

        FingerSnapSfx.PlayNormal();
    }
}
