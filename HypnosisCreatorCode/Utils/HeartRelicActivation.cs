using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Relics.Hearts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 希少な心臓の右クリック発動。UI の <see cref="NRelic"/> モデルと所持実体がずれないよう
/// <see cref="Player.Relics"/> から正本を解決する。
/// </summary>
public static class HeartRelicActivation
{
    private static bool _activating;

    private static readonly AccessTools.FieldRef<NRelicInventory, Player> InventoryPlayer =
        AccessTools.FieldRefAccess<NRelicInventory, Player>("_player");

    public static Player? ResolvePlayerFromHolder(NRelicInventoryHolder holder) => ResolvePlayer(holder);

    public static bool TryBeginFromHolder(NRelicInventoryHolder holder) =>
        TryBegin(ResolveOwnedHeart(holder.Relic?.Model, ResolvePlayer(holder)), ResolvePlayer(holder));

    public static bool TryBeginFromModel(RelicModel? model, Player? playerHint = null) =>
        TryBegin(ResolveOwnedHeart(model, playerHint ?? model?.Owner), playerHint ?? model?.Owner);

    public static bool CanActivateNow(EnemyHeartRelic heart, Player? player)
    {
        if (!heart.IsRareHeart || heart.IsUsedUp) return false;
        if (player == null) return false;
        if (heart.Owner != null && !ReferenceEquals(heart.Owner, player)) return false;

        var combat = CombatManager.Instance;
        return combat is { IsInProgress: true } && !combat.PlayerActionsDisabled;
    }

    public static bool ShouldHighlight(EnemyHeartRelic heart, Player? player) =>
        CanActivateNow(heart, player);

    private static bool TryBegin(EnemyHeartRelic? heart, Player? player)
    {
        if (_activating || heart == null || !CanActivateNow(heart, player)) return false;

        _activating = true;
        _ = ActivateThenUnlock(heart, player!);
        return true;
    }

    private static async Task ActivateThenUnlock(EnemyHeartRelic heart, Player player)
    {
        try
        {
            var ctx = new BlockingPlayerChoiceContext();
            await heart.ActivateAsync(ctx, player);
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

    private static Player? ResolvePlayer(NRelicInventoryHolder holder)
    {
        var model = holder.Relic?.Model;
        if (model?.Owner != null) return model.Owner;

        var inventory = holder.Inventory;
        return inventory == null ? null : InventoryPlayer(inventory);
    }

    /// <summary>
    /// UI 表示用モデルから、プレイヤー所持リスト上の正本を取る。
    /// </summary>
    public static EnemyHeartRelic? ResolveOwnedHeart(RelicModel? model, Player? player)
    {
        if (model is not EnemyHeartRelic) return null;
        if (player == null) return model as EnemyHeartRelic;

        var owned = player.Relics.OfType<EnemyHeartRelic>()
            .FirstOrDefault(r => r.Id.Entry == model.Id.Entry);
        return owned ?? model as EnemyHeartRelic;
    }
}
