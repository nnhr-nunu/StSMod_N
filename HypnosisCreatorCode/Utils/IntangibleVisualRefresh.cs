using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 実験体など、霊体付与時に <c>%CanvasGroup</c> の半透明を手動で付ける敵向け。
/// スライム催眠の見た目復帰（<see cref="SlimeDisguise.Restore"/>）後に霊体表示が消える再発を防ぐ。
/// </summary>
public static class IntangibleVisualRefresh
{
    public static void TryRefresh(Creature? creature)
    {
        if (creature is not { IsAlive: true }) return;
        if (creature.GetPower<IntangiblePower>() is not { Amount: > 0 }) return;

        try
        {
            var canvas = NCombatRoom.Instance?.GetCreatureNode(creature)?
                .GetSpecialNode<CanvasGroup>("%CanvasGroup");
            if (canvas != null)
                canvas.SetSelfModulate(StsColors.halfTransparentWhite);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Intangible visual refresh failed: {e.Message}");
        }
    }

    /// <summary>見た目差し替え直後は Animator 更新と同フレームで競合しうるため遅延再適用する。</summary>
    public static void TryRefreshDeferred(Creature? creature)
    {
        if (creature == null) return;
        Callable.From(() => TryRefresh(creature)).CallDeferred();
    }
}
