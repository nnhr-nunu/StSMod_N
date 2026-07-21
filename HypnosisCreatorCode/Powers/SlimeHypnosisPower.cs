using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// スライム催眠 — 1ターンだけ意図を粘液付与へ上書きし、見た目・名前をスライム系からランダム差し替え。
/// FollowUp と行動復元でステートマシン破壊（進行不能）を防ぐ。
/// </summary>
public class SlimeHypnosisPower : HypnosisCreatorPower
{
    public const string SlimeMoveId = "hypnosis_creator_slime_intent";

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public string? DisguiseName { get; private set; }
    private SlimeDisguise.State? _disguise;
    private MoveState? _savedMove;
    private bool _restoredMove;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        TryOverwriteIntent();
        TryApplyDisguise(applier);
        return Task.CompletedTask;
    }

    private void TryApplyDisguise(Creature? applier)
    {
        if (Owner == null) return;

        var rng = applier?.Player?.RunState.Rng.CombatCardSelection
                  ?? Owner.Monster?.CombatState?.Players.FirstOrDefault()?.RunState.Rng.CombatCardSelection;
        if (rng == null) return;

        _disguise = SlimeDisguise.Apply(Owner, rng);
        DisguiseName = _disguise?.DisplayName;
    }

    private void TryOverwriteIntent()
    {
        if (Owner?.Monster == null || CombatState == null) return;
        var count = Math.Max(1, Amount);
        var combat = CombatState;
        var monster = Owner.Monster;

        try
        {
            TryCaptureSavedMove(monster);

            async Task OnPerform(IReadOnlyList<Creature> targets)
            {
                foreach (var target in targets)
                {
                    var player = target.Player;
                    if (player == null) continue;
                    for (var i = 0; i < count; i++)
                    {
                        // 通常の粘液。状態異常催眠中は StatusHypnosisPower のフックで AbnormalSlime に置換される。
                        var slime = combat.CreateCard(ModelDb.Card<Slimed>(), player);
                        await CardPileCmd.AddGeneratedCardToCombat(slime, PileType.Discard, player);
                    }
                }
            }

            var move = new MoveState(SlimeMoveId, OnPerform, [new StatusIntent(count)]);
            // 実行後に保存した行動へ戻す（FollowUp 未設定だとステートマシンが壊れる）
            if (_savedMove != null && !IsOurSlimeMove(_savedMove))
                move.FollowUpState = _savedMove;
            else
                move.FollowUpState = move;

            monster.SetMoveImmediate(move, forceTransition: true);
        }
        catch
        {
            // Intent API 差異時はトランス付与のみで継続
        }
    }

    private void TryCaptureSavedMove(MonsterModel monster)
    {
        if (_savedMove != null) return;
        var current = monster.NextMove;
        if (current == null || IsOurSlimeMove(current)) return;
        _savedMove = current;
        _restoredMove = false;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (side != CombatSide.Enemy) return;
        if (!participants.Contains(Owner)) return;

        CleanupDisguise();
        TryRestoreSavedMove(Owner);
        await PowerCmd.Remove(this);
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        CleanupDisguise(oldOwner);
        TryRestoreSavedMove(oldOwner);
        return Task.CompletedTask;
    }

    private void CleanupDisguise(Creature? creature = null)
    {
        var target = creature ?? Owner;
        if (target != null)
            SlimeDisguise.Restore(target, _disguise);
        _disguise = null;
        DisguiseName = null;
    }

    private void TryRestoreSavedMove(Creature? creature)
    {
        if (_restoredMove) return;
        _restoredMove = true;

        var monster = creature?.Monster;
        if (monster == null || creature is not { IsAlive: true })
        {
            _savedMove = null;
            return;
        }

        // すでに粘液ムーブを実行済みで FollowUp に移っているなら触らない
        if (!IsOurSlimeMove(monster.NextMove))
        {
            _savedMove = null;
            return;
        }

        try
        {
            if (_savedMove != null && !IsOurSlimeMove(_savedMove))
                monster.SetMoveImmediate(_savedMove, forceTransition: true);
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

    private static bool IsOurSlimeMove(MoveState? move) =>
        move != null && move.StateId == SlimeMoveId;
}
