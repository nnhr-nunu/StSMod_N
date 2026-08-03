using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// マルチのターン終了ピン押下時、ヒプノクリエイターなら指パッチン SE を鳴らす。
/// 送信側は毎回（連打可）。受信側はネットメッセージ到着時に同期再生（みんなで演奏）。
/// セリフは characters.json の endTurnPing（本家 FlavorSynchronizer）。
/// </summary>
[HarmonyPatch(typeof(FlavorSynchronizer), nameof(FlavorSynchronizer.SendEndTurnPing))]
public static class EndTurnPingFingerSnapSendPatch
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

[HarmonyPatch(typeof(FlavorSynchronizer), "HandleEndTurnPingMessage")]
public static class EndTurnPingFingerSnapReceivePatch
{
    private static readonly FieldInfo? PlayerCollectionField =
        AccessTools.Field(typeof(FlavorSynchronizer), "_playerCollection");

    private static readonly FieldInfo? LocalPlayerIdField =
        AccessTools.Field(typeof(FlavorSynchronizer), "_localPlayerId");

    private static readonly MethodInfo? GetPlayerMethod =
        AccessTools.Method(typeof(IPlayerCollection), nameof(IPlayerCollection.GetPlayer), [typeof(ulong)]);

    public static void Postfix(FlavorSynchronizer __instance, EndTurnPingMessage message, ulong senderId)
    {
        _ = message;

        if (PlayerCollectionField == null || LocalPlayerIdField == null || GetPlayerMethod == null)
            return;

        var localPlayerId = (ulong)LocalPlayerIdField.GetValue(__instance)!;
        if (senderId == localPlayerId)
            return;

        var collection = PlayerCollectionField.GetValue(__instance);
        if (collection == null) return;

        var sender = GetPlayerMethod.Invoke(collection, [senderId]) as Player;
        if (!FingerSnapCardRules.IsHypnosisCreatorPlayer(sender)) return;

        FingerSnapSfx.PlayNormal();
    }
}
