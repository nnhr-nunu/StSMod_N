using Godot;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 希少な心臓をレリック枠の右クリックで発動する。
/// 左クリックは本家どおり Inspect 画面。
/// </summary>
[HarmonyPatch]
public static class RareHeartRightClickPatch
{
    private static bool _activating;

    [HarmonyPatch(typeof(NClickableControl), "_GuiInput")]
    [HarmonyPrefix]
    public static bool GuiInputPrefix(NClickableControl __instance, InputEvent @event)
    {
        if (__instance is not NRelicInventoryHolder holder) return true;
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
            return true;

        if (!TryBeginActivate(holder)) return true;

        holder.AcceptEvent();
        return false;
    }

    private static bool TryBeginActivate(NRelicInventoryHolder holder)
    {
        if (_activating) return false;

        var heart = holder.Relic?.Model as EnemyHeartRelic;
        if (heart == null || !heart.IsRareHeart || heart.IsUsedUp) return false;

        var player = heart.Owner;
        if (player == null) return false;

        var combat = CombatManager.Instance;
        if (combat is { IsInProgress: true, PlayerActionsDisabled: true }) return false;

        _activating = true;
        _ = ActivateThenUnlock(heart, player);
        return true;
    }

    private static async Task ActivateThenUnlock(EnemyHeartRelic heart, Player player)
    {
        try
        {
            // UI起点でも選択待ちに入れるよう Blocking を使う（Throwing だと一部 Cmd で落ちる）
            var ctx = new BlockingPlayerChoiceContext();
            await heart.ActivateAsync(ctx, player);
            // MarkUsed は各 ActivateAsync / Helper が「効果成功時のみ」行う。
            // 敵不在などで失敗した場合は使用済みにしない。
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Rare heart activate failed: {heart.Id.Entry}: {e}");
        }
        finally
        {
            _activating = false;
        }
    }
}
