using BaseLib.Utils;
using Godot;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 「引き寄せ（Pull）」— 対象をプレイヤー側へ寄せ、戦闘中は引き寄せ済みとして扱う。
/// 位置移動は本家 Sandpit と同様に global_position:x を Tween する。
/// 画面外への押し出し・プレイヤーとの過度な重なりをクランプする。
/// 寄せ／遠ざかりは使うたびに行い、移動量は回数ごとに半減する。
/// </summary>
public static class PullTracker
{
    /// <summary>初回の寄せる／遠ざける距離（ピクセル）。以降は半減。</summary>
    private const float PullDistance = 280f;

    /// <summary>ヒットボックス端同士の最低すき間。</summary>
    private const float MinEdgePadding = 48f;

    /// <summary>ヒットボックスが取れないときのフォールバック間隔（中心間）。</summary>
    private const float FallbackMinGapFromPlayer = 220f;

    /// <summary>プレイヤー中心からこれ以上遠くへは押し出さない。</summary>
    private const float MaxDistanceFromPlayer = 820f;

    /// <summary>画面端に残すマージン（スプライト半幅に加えて確保）。</summary>
    private const float ScreenEdgeMargin = 64f;

    private const float TweenSeconds = 0.35f;
    private const float MinMoveEpsilon = 4f;

    private static readonly NotNullSpireField<Creature, PullState> Field = new(() => new PullState());

    public static bool IsPulled(Creature creature) => Field.Get(creature).Pulled;

    /// <summary>
    /// 引き寄せアニメが可能か。背景一体型（カイザークラブ左右爪など）は NCreature だけ動き見た目が残る。
    /// </summary>
    public static bool CanPullVisually(Creature creature) =>
        creature is { IsEnemy: true } && !IntentOverwriteUnsafeMonsters.IsUnsafe(creature);

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
    /// プレイヤー側へ寄せる（毎回アニメ。移動量は寄せ回数ごとに半減）。
    /// 戻り値は「今回が初回の引き寄せか」（カード効果分岐用。フラグは初回で立つ）。
    /// Crusher / Rocket など背景一体型は見た目が動かないため、引き寄せ不可として扱う。
    /// </summary>
    public static async Task<bool> TryPull(Creature creature, Creature? towardPlayer)
    {
        if (!CanPullVisually(creature))
        {
            ShowCannotPullMessage(towardPlayer, creature);
            return false;
        }

        var state = Field.Get(creature);
        var wasFirst = !state.Pulled;
        state.Pulled = true;
        var distance = NextStepDistance(state.PullAnimCount);
        state.PullAnimCount++;
        await AnimateX(creature, towardPlayer, towardPlayer: true, distance);
        return wasFirst;
    }

    /// <summary>対象をプレイヤーから離す（決死の逃亡）。移動量は押し出し回数ごとに半減。引き寄せ済みフラグは変更しない。</summary>
    public static async Task TryPushAway(Creature creature, Creature? fromPlayer)
    {
        var state = Field.Get(creature);
        var distance = NextStepDistance(state.PushAnimCount);
        state.PushAnimCount++;
        await AnimateX(creature, fromPlayer, towardPlayer: false, distance);
    }

    private static float NextStepDistance(int priorCount)
    {
        // 0回目=100%、1回目=50%、2回目=25% …
        var distance = PullDistance * MathF.Pow(0.5f, priorCount);
        return Math.Max(distance, MinMoveEpsilon);
    }

    private static async Task AnimateX(Creature creature, Creature? player, bool towardPlayer, float distance)
    {
        if (IntentOverwriteUnsafeMonsters.IsUnsafe(creature))
            return;

        var room = NCombatRoom.Instance;
        if (room == null) return;

        var enemyNode = room.GetCreatureNode(creature);
        if (enemyNode == null || !GodotObject.IsInstanceValid(enemyNode)) return;

        var enemyX = enemyNode.GlobalPosition.X;
        float targetX;

        NCreature? playerNode = null;
        if (player != null)
            playerNode = room.GetCreatureNode(player);

        float? playerX = null;
        var minGap = FallbackMinGapFromPlayer;

        if (playerNode != null && GodotObject.IsInstanceValid(playerNode))
        {
            playerX = playerNode.GlobalPosition.X;
            minGap = GetMinCenterGap(playerNode, enemyNode);

            if (towardPlayer)
            {
                // 敵が右・プレイヤーが左が通常配置。左右どちらでもプレイヤーへ近づけるが、通過しない。
                if (enemyX >= playerX.Value)
                    targetX = Math.Max(playerX.Value + minGap, enemyX - distance);
                else
                    targetX = Math.Min(playerX.Value - minGap, enemyX + distance);
            }
            else
            {
                // プレイヤーから離す（画面外・過度な遠ざかりは後でクランプ）
                if (enemyX >= playerX.Value)
                    targetX = enemyX + distance;
                else
                    targetX = enemyX - distance;

                targetX = ClampAwayFromPlayer(playerX.Value, enemyX, targetX, minGap);
            }
        }
        else
        {
            targetX = towardPlayer ? enemyX - distance : enemyX + distance;
        }

        targetX = ClampToVisibleX(room, enemyNode, targetX);

        // 引き寄せのみ: 画面クランプでプレイヤー側へ押されても通過しない
        // 遠ざかりは画面端クランプを優先（何度逃亡しても画面外へ出さない）
        if (towardPlayer && playerX is float px)
            targetX = EnforcePlayerGapOnPull(px, enemyX, targetX, minGap);

        if (Math.Abs(targetX - enemyX) < MinMoveEpsilon) return;

        var tween = room.CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(enemyNode, "global_position:x", targetX, TweenSeconds);
        await TweenHelper.AwaitFinished(tween, room);
        ForcedSleep.NotifyCreatureMoved(creature);
    }

    /// <summary>プレイヤーを通り過ぎないよう、遠ざけ方向の上限も中心間隔で抑える。</summary>
    private static float ClampAwayFromPlayer(float playerX, float enemyX, float targetX, float minGap)
    {
        if (enemyX >= playerX)
        {
            var maxX = playerX + MaxDistanceFromPlayer;
            var minX = playerX + minGap;
            return Math.Clamp(targetX, minX, maxX);
        }

        var minLeft = playerX - MaxDistanceFromPlayer;
        var maxLeft = playerX - minGap;
        return Math.Clamp(targetX, minLeft, maxLeft);
    }

    /// <summary>画面クランプ後もプレイヤーとの最低間隔を守る（引き寄せ用）。</summary>
    private static float EnforcePlayerGapOnPull(
        float playerX, float enemyX, float targetX, float minGap)
    {
        if (enemyX >= playerX)
            return Math.Max(targetX, playerX + minGap);
        return Math.Min(targetX, playerX - minGap);
    }

    /// <summary>敵の体が画面内に残るよう X をクランプする。</summary>
    private static float ClampToVisibleX(NCombatRoom room, NCreature enemyNode, float targetX)
    {
        var body = GetBodyRect(enemyNode);
        var width = Math.Max(80f, body.Size.X);
        var pivotFromLeft = enemyNode.GlobalPosition.X - body.Position.X;

        if (!TryGetVisibleGlobalXRange(room, enemyNode, out var minVisible, out var maxVisible))
            return targetX;

        // 体の左端／右端が画面マージン内に収まるノード X
        var minX = minVisible + ScreenEdgeMargin + pivotFromLeft;
        var maxX = maxVisible - ScreenEdgeMargin - width + pivotFromLeft;
        if (minX > maxX)
            return (minX + maxX) * 0.5f;

        return Math.Clamp(targetX, minX, maxX);
    }

    private static bool TryGetVisibleGlobalXRange(NCombatRoom room, NCreature enemyNode, out float minX, out float maxX)
    {
        minX = 0f;
        maxX = 0f;

        try
        {
            var viewport = enemyNode.GetViewport() ?? room.GetViewport();
            if (viewport == null) return false;

            var localRect = viewport.GetVisibleRect();
            var xf = enemyNode.GetViewportTransform().AffineInverse();
            var p0 = xf * localRect.Position;
            var p1 = xf * (localRect.Position + localRect.Size);
            minX = Math.Min(p0.X, p1.X);
            maxX = Math.Max(p0.X, p1.X);
            return maxX - minX > 100f;
        }
        catch
        {
            return false;
        }
    }

    private static float GetMinCenterGap(NCreature playerNode, NCreature enemyNode)
    {
        var playerRect = GetBodyRect(playerNode);
        var enemyRect = GetBodyRect(enemyNode);
        var fromBoxes = playerRect.Size.X * 0.5f + enemyRect.Size.X * 0.5f + MinEdgePadding;
        return Math.Max(FallbackMinGapFromPlayer, fromBoxes);
    }

    private static Rect2 GetBodyRect(NCreature node)
    {
        if (node.Hitbox != null && GodotObject.IsInstanceValid(node.Hitbox))
        {
            var hit = node.Hitbox.GetGlobalRect();
            if (hit.Size.X > 1f)
                return hit;
        }

        var rect = node.GetGlobalRect();
        if (rect.Size.X > 1f)
            return rect;

        // 最終手段: 現在位置を中心にした仮の幅
        return new Rect2(node.GlobalPosition.X - 80f, node.GlobalPosition.Y - 80f, 160f, 160f);
    }

    private static void ShowCannotPullMessage(Creature? playerCreature, Creature target)
    {
        if (playerCreature is not { IsAlive: true }) return;

        var state = Field.Get(target);
        if (state.ShowedCannotPullHint) return;
        state.ShowedCannotPullHint = true;

        try
        {
            var text = ResolveCannotPullLine();
            var bubble = NSpeechBubbleVfx.Create(text, playerCreature, 1.5, VfxColor.White);
            if (bubble == null) return;

            var container = playerCreature.GetVfxContainer();
            if (container == null) return;
            GodotTreeExtensions.AddChildSafely(container, bubble);
        }
        catch (Exception e)
        {
            MainFile.Logger.Warn($"Pull blocked speech failed: {e.Message}");
        }
    }

    private static string ResolveCannotPullLine()
    {
        try
        {
            var text = new LocString("characters", "HYPNOSISCREATOR-HYPNOSIS_CREATOR.banter.pullBlocked")
                .GetFormattedText()?.Trim();
            if (!string.IsNullOrWhiteSpace(text)
                && !text.StartsWith("HYPNOSISCREATOR-", StringComparison.Ordinal))
                return text;
        }
        catch
        {
            // ignore
        }

        return UpgradeCardText.IsJapaneseUi()
            ? "この相手は引っ張れないようだ。"
            : "This foe doesn't seem like it can be pulled.";
    }
}

public sealed class PullState
{
    /// <summary>カード効果用（初回引き寄せ済みか）。アニメ回数とは独立。</summary>
    public bool Pulled { get; set; }

    public int PullAnimCount { get; set; }
    public int PushAnimCount { get; set; }

    /// <summary>背景一体型など引き寄せ不可の対象へ、戦闘中1回だけ吹き出しを出したか。</summary>
    public bool ShowedCannotPullHint { get; set; }
}
