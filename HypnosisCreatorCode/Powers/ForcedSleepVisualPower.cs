using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 睡眠中の見た目用（非表示）。
/// 睡眠前の <see cref="MonsterModel.NextMove"/> を保存し、睡眠中は SleepIntent の自己ループ、
/// 起床時に保存した行動予定へ戻す（カスタムムーブでステートマシンを壊さない）。
/// </summary>
public class ForcedSleepVisualPower : HypnosisCreatorPower
{
    public const string SleepMoveId = "hypnosis_creator_forced_sleep";

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    private MoveState? _savedMove;
    private NSleepingVfx? _vfx;
    private bool _ownsVfx;
    private bool _restored;

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

    /// <summary>睡眠スタック変動後。消えていれば予定を復元して片付ける。</summary>
    public Task OnAsleepAmountMaybeChangedAsync()
    {
        if (Owner == null) return Task.CompletedTask;
        if (Owner.HasPower<AsleepPower>()) return Task.CompletedTask;
        return CleanupAndRemove();
    }

    /// <summary>付与直後や再同期。すでに睡眠ムーブ中なら VFX だけ確認する。</summary>
    public void RefreshPresentation()
    {
        if (Owner?.Monster == null || !Owner.IsAlive) return;
        if (!Owner.HasPower<AsleepPower>()) return;

        if (IsOurSleepMove(Owner.Monster.NextMove))
        {
            TryStartVfx();
            return;
        }

        BeginSleepPresentation();
    }

    private void BeginSleepPresentation()
    {
        if (Owner?.Monster == null || !Owner.IsAlive) return;
        if (!Owner.HasPower<AsleepPower>()) return;

        TryCaptureSavedMove(Owner.Monster);
        TryForceSleepMove(Owner.Monster);
        TryStartVfx();
    }

    private void TryCaptureSavedMove(MonsterModel monster)
    {
        if (_savedMove != null) return;

        var current = monster.NextMove;
        if (current == null || IsOurSleepMove(current)) return;

        _savedMove = current;
        _restored = false;
    }

    private void TryForceSleepMove(MonsterModel monster)
    {
        try
        {
            static Task OnPerform(IReadOnlyList<Creature> _) => Task.CompletedTask;

            var sleep = new MoveState(SleepMoveId, OnPerform, [new SleepIntent()]);
            // 実行後も睡眠に留まり、FollowUp 未設定によるステートマシン破壊を防ぐ
            sleep.FollowUpState = sleep;
            monster.SetMoveImmediate(sleep, forceTransition: true);
        }
        catch
        {
            // Intent API 差異時は Asleep 本体のみ
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
            if (_savedMove != null && !IsOurSleepMove(_savedMove))
            {
                monster.SetMoveImmediate(_savedMove, forceTransition: true);
            }
            else
            {
                // 保存が無い／壊れているときは通常ロールに戻す
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

    private static bool IsOurSleepMove(MoveState? move) =>
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

            var vfx = NSleepingVfx.Create(spawnPos, goingRight: true);
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
            // VFX 失敗時は意図上書きのみ
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
