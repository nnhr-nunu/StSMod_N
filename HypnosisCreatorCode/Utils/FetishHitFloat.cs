using Godot;
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
    /// <summary>本家 NSpeechBubbleVfx.GetCreatureSpeechPosition と同じ比率。</summary>
    private const float SpawnProportionToTopOfHitbox = 0.75f;
    private const float SpawnProportionToEdgeOfHitbox = 0.75f;

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

            var node = target.GetCreatureNode();
            var side = ResolveDialogueSide(target, node);
            var position = ResolveSpeechPosition(node, side);

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

    /// <summary>
    /// 口元アンカー。引き寄せ／縮小／スライム差し替え後も、いまの Hitbox に追従する。
    /// TalkPos がヒットボックスから大きく外れる敵（ソウルネクサス等）は Hitbox 計算に落とす。
    /// </summary>
    private static Vector2 ResolveSpeechPosition(NCreature? node, DialogueSide side)
    {
        if (node == null) return Vector2.Zero;

        try
        {
            var talk = node.Visuals?.TalkPosition;
            if (talk != null && GodotObject.IsInstanceValid(talk))
            {
                var gp = talk.GlobalPosition;
                if (IsPlausibleTalkAnchor(node, gp))
                    return gp;
            }
        }
        catch
        {
            // TalkPosition 不正時は Hitbox へ
        }

        return GetHitboxSpeechPosition(node, side);
    }

    /// <summary>
    /// 本家 GetCreatureSpeechPosition のフォールバックと同型。
    /// X オフセットは吹き出しの開き方向（DialogueSide）に合わせ、左右配置の敵でも口元側に付く。
    /// </summary>
    private static Vector2 GetHitboxSpeechPosition(NCreature node, DialogueSide side)
    {
        var hit = node.Hitbox;
        var result = node.VfxSpawnPosition
                     + new Vector2(0f, -hit.Size.Y * 0.5f * SpawnProportionToTopOfHitbox);

        // Left = 吹き出しが右へ伸びる → 口元は右端寄り / Right = 左へ伸びる → 左端寄り
        if (side == DialogueSide.Left)
            result.X += hit.Size.X * SpawnProportionToEdgeOfHitbox;
        else
            result.X -= hit.Size.X * SpawnProportionToEdgeOfHitbox;

        return result;
    }

    /// <summary>TalkPos が現在の見た目ヒットボックス近傍にあるか（ずれマーカー除外）。</summary>
    private static bool IsPlausibleTalkAnchor(NCreature node, Vector2 globalTalk)
    {
        var hit = node.Hitbox;
        if (hit == null || !GodotObject.IsInstanceValid(hit)) return false;
        if (hit.Size.X <= 1f || hit.Size.Y <= 1f) return false;

        var pad = Math.Max(hit.Size.X, hit.Size.Y) * 0.4f;
        var rect = new Rect2(hit.GlobalPosition, hit.Size).Grow(pad);
        return rect.HasPoint(globalTalk);
    }

    /// <summary>
    /// 画面中央側へ吹き出しを出す。
    /// Left = 右方向へ伸びる / Right = 左方向へ伸びる（本家 NSpeechBubbleVfx._Ready）。
    /// </summary>
    private static DialogueSide ResolveDialogueSide(Creature enemy, NCreature? enemyNode)
    {
        if (enemyNode == null) return DialogueSide.Right;

        try
        {
            var player = enemy.CombatState?.GetOpponentsOf(enemy).FirstOrDefault();
            var playerNode = player != null
                ? NCombatRoom.Instance?.GetCreatureNode(player)
                : null;
            if (playerNode == null) return DialogueSide.Right;

            // プレイヤーより左 → 右へ伸ばす（中央側） / 右 → 左へ伸ばす（本家 Enemy 既定）
            return enemyNode.GlobalPosition.X < playerNode.GlobalPosition.X
                ? DialogueSide.Left
                : DialogueSide.Right;
        }
        catch
        {
            return DialogueSide.Right;
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
