using BaseLib.Utils;
using Godot;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 「引き寄せ（Pull）」— 対象をプレイヤー側へ寄せ、戦闘中は引き寄せ済みとして扱う。
/// 位置移動は本家 Sandpit と同様に global_position:x を Tween する。
/// </summary>
public static class PullTracker
{
    /// <summary>一度に寄せる距離（ピクセル）。</summary>
    private const float PullDistance = 280f;

    /// <summary>プレイヤーと重ならないよう残す最低間隔。</summary>
    private const float MinGapFromPlayer = 150f;

    private const float TweenSeconds = 0.35f;

    private static readonly NotNullSpireField<Creature, PullState> Field = new(() => new PullState());

    public static bool IsPulled(Creature creature) => Field.Get(creature).Pulled;

    /// <summary>
    /// ぬぬ地獄所持時、引き寄せ系カードの対象へ追加ダメージ。
    /// </summary>
    public static async Task TryNunuHellBonusDamageAsync(
        PlayerChoiceContext choiceContext,
        Creature dealer,
        Creature target,
        CardModel? cardSource)
    {
        if (!dealer.IsAlive || !target.IsAlive) return;
        var power = dealer.GetPower<NunuHellPower>();
        if (power == null || power.Amount <= 0) return;

        await CreatureCmd.Damage(
            choiceContext, target, power.Amount, ValueProp.Unpowered, dealer, cardSource, null);
    }

    /// <summary>
    /// まだ引き寄せていなければプレイヤー側へ動かして true。
    /// 既に引き寄せ済みなら false。
    /// </summary>
    public static async Task<bool> TryPull(Creature creature, Creature? towardPlayer)
    {
        var state = Field.Get(creature);
        if (state.Pulled) return false;
        state.Pulled = true;
        await AnimateX(creature, towardPlayer, towardPlayer: true);
        return true;
    }

    /// <summary>対象をプレイヤーから離す（決死の逃亡）。引き寄せ済みフラグは変更しない。</summary>
    public static async Task TryPushAway(Creature creature, Creature? fromPlayer)
    {
        await AnimateX(creature, fromPlayer, towardPlayer: false);
    }

    private static async Task AnimateX(Creature creature, Creature? player, bool towardPlayer)
    {
        var room = NCombatRoom.Instance;
        if (room == null) return;

        var enemyNode = room.GetCreatureNode(creature);
        if (enemyNode == null || !GodotObject.IsInstanceValid(enemyNode)) return;

        var enemyX = enemyNode.GlobalPosition.X;
        float targetX;

        NCreature? playerNode = null;
        if (player != null)
            playerNode = room.GetCreatureNode(player);

        if (playerNode != null && GodotObject.IsInstanceValid(playerNode))
        {
            var playerX = playerNode.GlobalPosition.X;
            if (towardPlayer)
            {
                // 敵が右・プレイヤーが左が通常配置。左右どちらでもプレイヤーへ近づける。
                if (enemyX >= playerX)
                    targetX = Math.Max(playerX + MinGapFromPlayer, enemyX - PullDistance);
                else
                    targetX = Math.Min(playerX - MinGapFromPlayer, enemyX + PullDistance);
            }
            else
            {
                // プレイヤーから離す
                if (enemyX >= playerX)
                    targetX = enemyX + PullDistance;
                else
                    targetX = enemyX - PullDistance;
            }
        }
        else
        {
            targetX = towardPlayer ? enemyX - PullDistance : enemyX + PullDistance;
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
