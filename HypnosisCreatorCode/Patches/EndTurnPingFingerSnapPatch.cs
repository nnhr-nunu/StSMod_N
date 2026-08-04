using System.Reflection;
using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Game.Flavor;
using MegaCrit.Sts2.Core.Runs;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// ヒプノクリエイターのターン終了ピン — 指パッチン SE を連打で全員に同期。
/// EndTurnPingMessage を毎押下で配信し、吹き出しだけ本家同様 1 秒に 1 回に制限する。
/// </summary>
[HarmonyPatch(typeof(FlavorSynchronizer), nameof(FlavorSynchronizer.SendEndTurnPing))]
public static class EndTurnPingFingerSnapSendPatch
{
    private static readonly PropertyInfo? LocalPlayerProperty =
        AccessTools.Property(typeof(FlavorSynchronizer), "LocalPlayer");

    private static readonly FieldInfo? GameServiceField =
        AccessTools.Field(typeof(FlavorSynchronizer), "_gameService");

    private static readonly FieldInfo? NextAllowedPingTimeField =
        AccessTools.Field(typeof(FlavorSynchronizer), "_nextAllowedPingTime");

    public static bool Prefix(FlavorSynchronizer __instance)
    {
        if (LocalPlayerProperty == null) return true;

        var localPlayer = LocalPlayerProperty.GetValue(__instance) as Player;
        if (!FingerSnapCardRules.IsHypnosisCreatorPlayer(localPlayer))
            return true;

        FingerSnapSfx.PlayNormal();

        var gameService = GameServiceField?.GetValue(__instance) as INetGameService;
        var now = Time.GetTicksMsec();
        var nextAllowed = NextAllowedPingTimeField == null
            ? now
            : (ulong)NextAllowedPingTimeField.GetValue(__instance)!;

        // クールダウン中だけ追加送信する。本家送信時は本家の吹き出し処理を通す。
        if (now < nextAllowed && gameService is { IsConnected: true })
        {
            gameService.SendMessage(default(EndTurnPingMessage));
        }

        return true;
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

