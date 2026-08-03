using System.Collections.Generic;
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

    private static readonly MethodInfo? CreateDialogueMethod =
        AccessTools.Method(typeof(FlavorSynchronizer), "CreateEndTurnPingDialogueIfNecessary");

    public static bool Prefix(FlavorSynchronizer __instance)
    {
        if (LocalPlayerProperty == null) return true;

        var localPlayer = LocalPlayerProperty.GetValue(__instance) as Player;
        if (!FingerSnapCardRules.IsHypnosisCreatorPlayer(localPlayer))
            return true;

        FingerSnapSfx.PlayNormal();

        var gameService = GameServiceField?.GetValue(__instance) as INetGameService;
        if (gameService is { IsConnected: true })
            gameService.SendMessage(default(EndTurnPingMessage));

        var now = (long)Time.GetTicksMsec();
        if (NextAllowedPingTimeField != null && CreateDialogueMethod != null)
        {
            var nextAllowed = (long)NextAllowedPingTimeField.GetValue(__instance)!;
            if (now >= nextAllowed)
            {
                NextAllowedPingTimeField.SetValue(__instance, now + 1000);
                CreateDialogueMethod.Invoke(__instance, [localPlayer]);
            }
        }

        return false;
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

/// <summary>
/// 連打配信時に吹き出しだけプレイヤーごと 1 秒に 1 回へ制限（SE は毎回）。
/// </summary>
[HarmonyPatch(typeof(FlavorSynchronizer), "CreateEndTurnPingDialogueIfNecessary")]
public static class EndTurnPingDialogueDebouncePatch
{
    private const long DebounceMsec = 1000;

    private static readonly Dictionary<ulong, long> LastDialogueMsecByNetId = new();

    public static bool Prefix(Player player)
    {
        var netId = player.NetId;
        var now = (long)Time.GetTicksMsec();
        if (LastDialogueMsecByNetId.TryGetValue(netId, out var last) && now - last < DebounceMsec)
            return false;

        LastDialogueMsecByNetId[netId] = now;
        return true;
    }
}
