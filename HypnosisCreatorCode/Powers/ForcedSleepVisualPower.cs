using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 睡眠中の見た目用（非表示）。行動予定を SleepIntent にし、ZZZ／ぐうぐう VFX を出す。
/// ターン開始中に SetMoveImmediate すると進行不能になるため、付与時と敵ターン終了後だけ意図を更新する。
/// </summary>
public class ForcedSleepVisualPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    private NSleepingVfx? _vfx;
    private bool _ownsVfx;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        RefreshPresentation();
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        // ターン開始フックでは意図上書きも Remove もしない（進行宙吊り防止）
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || !participants.Contains(Owner)) return;
        if (side != CombatSide.Enemy) return;

        if (!Owner.HasPower<AsleepPower>())
        {
            await CleanupAndRemove();
            return;
        }

        // 敵の行動が終わったあとなら安全に、次周の睡眠意図を載せ直す
        RefreshPresentation();
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        StopOwnedVfx();
        return Task.CompletedTask;
    }

    /// <summary>睡眠スタック変動後。消えていれば VFX／意図用パワーを片付ける（呼び出し側で await すること）。</summary>
    public Task OnAsleepAmountMaybeChangedAsync()
    {
        if (Owner == null) return Task.CompletedTask;
        if (Owner.HasPower<AsleepPower>()) return Task.CompletedTask;
        return CleanupAndRemove();
    }

    public void RefreshPresentation()
    {
        if (Owner?.Monster == null || !Owner.IsAlive) return;
        if (!Owner.HasPower<AsleepPower>()) return;

        TryForceSleepMove();
        TryStartVfx();
    }

    private void TryForceSleepMove()
    {
        if (Owner?.Monster == null) return;
        try
        {
            static Task OnPerform(IReadOnlyList<Creature> _) => Task.CompletedTask;

            var move = new MoveState(
                "hypnosis_creator_forced_sleep",
                OnPerform,
                [new SleepIntent()]);
            Owner.Monster.SetMoveImmediate(move, forceTransition: true);
        }
        catch
        {
            // Intent API 差異時は Asleep 本体のみで継続
        }
    }

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
