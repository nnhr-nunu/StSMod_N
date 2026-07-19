using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 「引き寄せ（Pull）」— 対象をプレイヤー側へ寄せ、戦闘中は引き寄せ済みとして扱う。
/// 位置移動は本家 Sandpit と同様に global_position:x を Tween する。
/// </summary>
public static class PullTracker
{
    /// <summary>マウビースト心臓など、引き寄せ系効果の追加ダメージ（暫定）。</summary>
    public static int ExtraDamage { get; set; }

    /// <summary>一度に寄せる距離（ピクセル）。</summary>
    private const float PullDistance = 280f;

    /// <summary>プレイヤーと重ならないよう残す最低間隔。</summary>
    private const float MinGapFromPlayer = 150f;

    private const float TweenSeconds = 0.35f;

    private static readonly NotNullSpireField<Creature, PullState> Field = new(() => new PullState());

    public static bool IsPulled(Creature creature) => Field.Get(creature).Pulled;

    /// <summary>
    /// まだ引き寄せていなければプレイヤー側へ動かして true。
    /// 既に引き寄せ済みなら false。
    /// </summary>
    public static async Task<bool> TryPull(Creature creature, Creature? towardPlayer)
    {
        var state = Field.Get(creature);
        if (state.Pulled) return false;
        state.Pulled = true;
        await AnimateTowardPlayer(creature, towardPlayer);
        return true;
    }

    private static async Task AnimateTowardPlayer(Creature creature, Creature? towardPlayer)
    {
        var room = NCombatRoom.Instance;
        if (room == null) return;

        var enemyNode = room.GetCreatureNode(creature);
        if (enemyNode == null || !GodotObject.IsInstanceValid(enemyNode)) return;

        var enemyX = enemyNode.GlobalPosition.X;
        float targetX;

        NCreature? playerNode = null;
        if (towardPlayer != null)
            playerNode = room.GetCreatureNode(towardPlayer);

        if (playerNode != null && GodotObject.IsInstanceValid(playerNode))
        {
            var playerX = playerNode.GlobalPosition.X;
            // 敵が右・プレイヤーが左が通常配置。左右どちらでもプレイヤーへ近づける。
            if (enemyX >= playerX)
                targetX = Math.Max(playerX + MinGapFromPlayer, enemyX - PullDistance);
            else
                targetX = Math.Min(playerX - MinGapFromPlayer, enemyX + PullDistance);
        }
        else
        {
            // ノード未取得時は左（プレイヤー側）へ寄せる
            targetX = enemyX - PullDistance;
        }

        if (Math.Abs(targetX - enemyX) < 4f) return;

        var tween = room.CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(enemyNode, "global_position:x", targetX, TweenSeconds);
        await TweenHelper.AwaitFinished(tween, room);
    }
}

public sealed class PullState
{
    public bool Pulled { get; set; }
}
