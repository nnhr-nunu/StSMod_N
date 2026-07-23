using Godot;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
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

            // 左配置の敵はデフォルト（Enemy→左吹き出し）だと画面外へ出るため、向きを反転する。
            var node = target.GetCreatureNode();
            var position = ResolveSpeechPosition(target, node);
            var side = ResolveDialogueSide(target, node);

            var bubble = NSpeechBubbleVfx.Create(text, side, position, 1.25, VfxColor.Gold);
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

    private static Vector2 ResolveSpeechPosition(Creature target, NCreature? node)
    {
        var slimeSpeech = target.GetPower<SlimeHypnosisPower>()?.TryGetDisguiseSpeechPosition();
        if (slimeSpeech is Vector2 disguised)
            return disguised;

        try
        {
            var talk = node?.Visuals?.TalkPosition;
            if (talk != null && GodotObject.IsInstanceValid(talk))
                return talk.GlobalPosition;
        }
        catch
        {
            // TalkPosition が無い敵はフォールバック
        }

        if (node != null)
            return node.VfxSpawnPosition;

        return Vector2.Zero;
    }

    /// <summary>
    /// プレイヤーより左の敵 → 吹き出しを右向き（画面中央側）。
    /// 右の敵 → 左向き（従来どおりプレイヤー側）。
    /// </summary>
    private static DialogueSide ResolveDialogueSide(Creature enemy, NCreature? enemyNode)
    {
        if (enemyNode == null) return DialogueSide.Left;

        try
        {
            var player = enemy.CombatState?.GetOpponentsOf(enemy).FirstOrDefault();
            var playerNode = player != null
                ? NCombatRoom.Instance?.GetCreatureNode(player)
                : null;
            if (playerNode == null) return DialogueSide.Left;

            return enemyNode.GlobalPosition.X < playerNode.GlobalPosition.X
                ? DialogueSide.Right
                : DialogueSide.Left;
        }
        catch
        {
            return DialogueSide.Left;
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
