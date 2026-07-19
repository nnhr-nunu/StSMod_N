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
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 睡眠中の見た目用（非表示）。行動予定を SleepIntent にし、ZZZ／ぐうぐう VFX を出す。
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
        if (Owner == null || !participants.Contains(Owner)) return Task.CompletedTask;
        if (side != CombatSide.Enemy) return Task.CompletedTask;

        if (!Owner.HasPower<AsleepPower>())
            return CleanupAndRemove();

        RefreshPresentation();
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || !participants.Contains(Owner)) return Task.CompletedTask;
        if (side != CombatSide.Enemy) return Task.CompletedTask;

        if (!Owner.HasPower<AsleepPower>())
            return CleanupAndRemove();

        // 次ターンも寝ている見た目を維持（ムーブ実行後にステートマシンが次意図を選ぶのを抑止）
        RefreshPresentation();
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || Owner == null) return Task.CompletedTask;
        // Asleep 起床とフック順が前後しても、非ガードダメージ／睡眠消失で表示を消す
        if (result.UnblockedDamage > 0 || !Owner.HasPower<AsleepPower>())
            return CleanupAndRemove();
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        StopOwnedVfx();
        return Task.CompletedTask;
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
