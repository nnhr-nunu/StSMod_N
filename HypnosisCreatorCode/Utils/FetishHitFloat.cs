using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using HcMain = HypnosisCreator.HypnosisCreatorCode.MainFile;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>性癖刺さり成功時の吹き出し（フレーバー文言をランダム表示）。</summary>
public static class FetishHitFloat
{
    private static readonly string[] JapaneseLines =
    [
        "性癖に刺さった！",
        "助かる～",
        "ちょうど切らしてた",
        "推しが尊い",
        "ありがたや……",
        "（†昇天†）",
        "わ、わぁ…",
        "沼",
        "尊い……",
        "スパチャしなきゃ…",
        "ひぎぃ",
        "もっと……",
        "！！！",
        "顔ない",
        "……！",
        "尊死",
        "好きすぎてしんどい",
        "推ししか勝たん",
        "顔が良すぎる",
    ];

    private static readonly string[] EnglishLines =
    [
        "Fetish hit!",
        "Lifesaver~",
        "I was running low!",
        "My oshi is sacred",
        "I'm so grateful...",
        "(†Ascended†)",
        "W-woah...",
        "Bog",
        "So precious...",
        "Gotta superchat...",
        "Hgyah",
        "More...",
        "!!!",
        "No face left",
        "...!",
        "Oshideath",
        "Love them so much it hurts",
        "Only oshi wins",
        "Face too good",
    ];

    public static void Show(Creature target)
    {
        if (target is not { IsAlive: true, IsEnemy: true }) return;

        try
        {
            var lines = UpgradeCardText.IsJapaneseUi() ? JapaneseLines : EnglishLines;
            var text = lines[PickIndex(target, lines.Length)];

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

    private static int PickIndex(Creature target, int count)
    {
        if (count <= 1) return 0;

        var player = target.CombatState?.Players.FirstOrDefault();
        var rng = player?.RunState?.Rng?.CombatCardSelection;
        if (rng != null)
            return rng.NextInt(count);

        return Random.Shared.Next(count);
    }
}
