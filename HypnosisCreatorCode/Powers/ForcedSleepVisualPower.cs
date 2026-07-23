using Godot;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 睡眠中の見た目用（非表示）。
/// 睡眠前の <see cref="MonsterModel.NextMove"/> を保存し、睡眠中は PerformMove / PerformIntent をスキップして
/// ターン進行だけ完了させる。起床時に保存した行動予定へ戻す。
/// SetMoveImmediate による睡眠ムーブ差し替えは、蠢く群生体等で NextMove が空になり進行不能になるため使わない。
/// </summary>
public class ForcedSleepVisualPower : HypnosisCreatorPower
{
    public const string SleepMoveId = "hypnosis_creator_forced_sleep";

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    /// <summary>睡眠中は敵ターンの PerformMove / PerformIntent を止める。</summary>
    public bool ShouldSkipPerform =>
        _skipPerformMode && Owner?.HasPower<AsleepPower>() == true;

    private MoveState? _savedMove;
    private NSleepingVfx? _vfx;
    private bool _ownsVfx;
    private bool _restored;
    private bool _skipPerformMode;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        BeginSleepPresentation();
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        TryRestoreSavedMove(oldOwner);
        StopOwnedVfx();
        return Task.CompletedTask;
    }

    /// <summary>睡眠スタック変動後。継続中はスキップ経路を維持し、消えていれば予定を復元して片付ける。</summary>
    public Task OnAsleepAmountMaybeChangedAsync()
    {
        if (Owner == null) return Task.CompletedTask;
        if (Owner.HasPower<AsleepPower>())
        {
            EnsureSleepSkipReady();
            return Task.CompletedTask;
        }

        return CleanupAndRemove();
    }

    /// <summary>付与直後や再同期。</summary>
    public void RefreshPresentation()
    {
        if (Owner?.Monster == null || !Owner.IsAlive) return;
        if (!Owner.HasPower<AsleepPower>()) return;

        EnsureSleepSkipReady();
    }

    private void BeginSleepPresentation()
    {
        if (Owner?.Monster == null || !Owner.IsAlive) return;
        if (!Owner.HasPower<AsleepPower>()) return;

        EnsureSleepSkipReady();
    }

    private void EnsureSleepSkipReady()
    {
        if (Owner?.Monster == null || !Owner.IsAlive) return;
        if (!Owner.HasPower<AsleepPower>()) return;

        TryCaptureSavedMove(Owner.Monster);
        _skipPerformMode = true;
        EnsureNextMoveForSkip(Owner.Monster);
        TryStartVfx();
    }

    private void TryCaptureSavedMove(MonsterModel monster)
    {
        if (_savedMove != null) return;

        var current = monster.NextMove;
        if (current == null || IsLegacySleepMove(current)) return;

        _savedMove = current;
        _restored = false;
    }

    /// <summary>スキップ経路で OnMovePerformed できるよう、行動予定が空ならロールする。</summary>
    private static void EnsureNextMoveForSkip(MonsterModel monster)
    {
        if (monster.NextMove != null) return;

        try
        {
            var creature = monster.Creature;
            if (creature is not { IsAlive: true }) return;
            var targets = creature.CombatState?.GetOpponentsOf(creature) ?? [];
            monster.RollMove(targets);
        }
        catch
        {
            // ロール失敗時はバニラ側の次ターン処理に委ねる
        }
    }

    private void TryRestoreSavedMove(Creature? creature)
    {
        if (_restored) return;
        _restored = true;

        var monster = creature?.Monster;
        if (monster == null || creature is not { IsAlive: true })
        {
            _savedMove = null;
            return;
        }

        try
        {
            if (_savedMove != null && !IsLegacySleepMove(_savedMove))
            {
                monster.SetMoveImmediate(_savedMove, forceTransition: true);
            }
            else
            {
                var targets = creature.CombatState?.GetOpponentsOf(creature) ?? [];
                monster.RollMove(targets);
            }
        }
        catch
        {
            try
            {
                var targets = creature.CombatState?.GetOpponentsOf(creature) ?? [];
                monster.RollMove(targets);
            }
            catch
            {
                // 復元失敗時はバニラ側の次ロールに委ねる
            }
        }
        finally
        {
            _savedMove = null;
        }
    }

    private static bool IsLegacySleepMove(MoveState? move) =>
        move != null && move.StateId == SleepMoveId;

    private void TryStartVfx()
    {
        if (Owner?.Monster == null || _vfx != null) return;

        try
        {
            var creatureNode = Owner.GetCreatureNode();
            if (creatureNode == null) return;

            var marker = creatureNode.GetSpecialNode<Node2D>("%SleepVfxPos");
            var spawnPos = marker != null ? marker.GlobalPosition : creatureNode.VfxSpawnPosition;

            // 左側配置（クラッシャー等）はプレイヤー側（右）へ向ける
            var goingRight = true;
            var player = Owner.CombatState?.GetOpponentsOf(Owner).FirstOrDefault();
            var playerNode = player != null
                ? MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance?.GetCreatureNode(player)
                : null;
            if (playerNode != null)
                goingRight = creatureNode.GlobalPosition.X < playerNode.GlobalPosition.X;

            var vfx = NSleepingVfx.Create(spawnPos, goingRight);
            if (vfx == null) return;

            var parent = (Node?)marker
                         ?? Owner.GetVfxContainer()
                         ?? creatureNode;
            GodotTreeExtensions.AddChildSafely(parent, vfx);
            if (marker != null)
                vfx.Position = Vector2.Zero;

            _vfx = vfx;
            _ownsVfx = true;
        }
        catch
        {
            // VFX 失敗時はスキップ経路のみ
        }
    }

    private async Task CleanupAndRemove()
    {
        TryRestoreSavedMove(Owner);
        StopOwnedVfx();
        if (Owner?.GetPower<ForcedSleepVisualPower>() == this)
            await PowerCmd.Remove(this);
    }

    private void StopOwnedVfx()
    {
        if (!_ownsVfx || _vfx == null) return;
        try
        {
            _vfx.Stop();
        }
        catch
        {
            // ノード破棄済みなど
        }

        _vfx = null;
        _ownsVfx = false;
    }
}
