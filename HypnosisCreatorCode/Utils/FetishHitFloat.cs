using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using HcMain = HypnosisCreator.HypnosisCreatorCode.MainFile;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>性癖刺さり成功時の吹き出し（性癖に刺さった！）。</summary>
public static class FetishHitFloat
{
    public static void Show(Creature target)
    {
        if (target is not { IsAlive: true, IsEnemy: true }) return;

        try
        {
            var text = UpgradeCardText.IsJapaneseUi()
                ? "性癖に刺さった！"
                : "Fetish hit!";

            var bubble = NSpeechBubbleVfx.Create(text, target, 1.25, VfxColor.Gold);
            if (bubble == null) return;

            var container = target.GetVfxContainer();
            if (container == null) return;
            GodotTreeExtensions.AddChildSafely(container, bubble);
        }
        catch (Exception e)
        {
            HcMain.Logger.Warn($"FetishHitFloat failed: {e.Message}");
        }
    }
}
