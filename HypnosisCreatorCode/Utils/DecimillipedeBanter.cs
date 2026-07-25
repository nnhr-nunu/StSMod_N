using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 万足ムカデ（本家 Reattach）への引き寄せ時のプレイヤー吹き出し。
/// 縮小は <c>CanChangeScale=false</c> のため見た目が変わらず、セリフは出さない。
/// </summary>
public static class DecimillipedeBanter
{
    private const string LocKey = "HYPNOSISCREATOR-HYPNOSIS_CREATOR.banter.decimillipedeStillMoving";

    public static bool IsDecimillipede(Creature? creature)
    {
        if (creature is not { IsEnemy: true }) return false;
        if (creature.HasPower<ReattachPower>()) return true;

        var id = HeartRegistry.GetMonsterId(creature);
        return id != null
            && id.StartsWith("DECIMILLIPEDE", StringComparison.OrdinalIgnoreCase);
    }

    public static void TryShowStillMovingBanter(Creature? playerCreature)
    {
        if (playerCreature is not { IsAlive: true, IsPlayer: true }) return;

        try
        {
            var text = ResolveLine();
            var bubble = NSpeechBubbleVfx.Create(text, playerCreature, 1.5, VfxColor.White);
            if (bubble == null) return;

            var container = playerCreature.GetVfxContainer();
            if (container == null) return;
            GodotTreeExtensions.AddChildSafely(container, bubble);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Decimillipede banter failed: {e.Message}");
        }
    }

    private static string ResolveLine()
    {
        try
        {
            var text = new LocString("characters", LocKey).GetFormattedText()?.Trim();
            if (!string.IsNullOrWhiteSpace(text)
                && !text.StartsWith("HYPNOSISCREATOR-", StringComparison.Ordinal))
                return text;
        }
        catch
        {
            // ignore
        }

        return UpgradeCardText.IsJapaneseUi()
            ? "千切れても動いている…！"
            : "It's still moving, even torn apart...!";
    }
}
