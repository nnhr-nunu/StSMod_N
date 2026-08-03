using System.Runtime.CompilerServices;
using BaseLib.Utils;
using Godot;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>見た目スライム化をスキップしたとき、プレイヤー吹き出しで一度だけ通知する。</summary>
public static class SlimeDisguiseBanter
{
    private const string LocKey = "HYPNOSISCREATOR-HYPNOSIS_CREATOR.banter.slimeDisguiseBlocked";

    private static readonly ConditionalWeakTable<Creature, HintState> HintTable = new();

    public static void TryShowBlocked(Creature? applier, Creature target)
    {
        if (!IntentOverwriteUnsafeMonsters.SkipsVisualDisguise(target)) return;

        var playerCreature = ResolvePlayerCreature(applier, target);
        if (playerCreature is not { IsAlive: true }) return;

        var state = HintTable.GetOrCreateValue(target);
        if (state.Shown) return;
        state.Shown = true;

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
            MainFile.Logger.Warn($"Slime disguise blocked speech failed: {e.Message}");
        }
    }

    private static Creature? ResolvePlayerCreature(Creature? applier, Creature target)
    {
        if (applier is { IsPlayer: true, IsAlive: true }) return applier;
        return target.CombatState?.Players.FirstOrDefault()?.Creature;
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
            ? "この相手はスライム化しないようだ……！"
            : "This foe doesn't seem to turn into slime...!";
    }

    private sealed class HintState
    {
        public bool Shown;
    }
}
