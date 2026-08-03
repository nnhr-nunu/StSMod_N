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
/// スライム催眠 — 意図を粘液付与へ上書きし、見た目・名前をスライム系からランダム差し替え。
/// 重ねがけで残り敵ターン+1（次の敵ターンも粘液意図）。
/// FollowUp と行動復元でステートマシン破壊（進行不能）を防ぐ。
/// Crusher / Rocket は意図ステートを書き換えず、PerformMove 差し替えで同等効果にする。
/// </summary>
public class SlimeHypnosisPower : HypnosisCreatorPower
{
    public const string SlimeMoveId = "hypnosis_creator_slime_intent";

    private const float SlimeAttackAnimDelay = 0.65f;

    public override PowerType Type => PowerType.Debuff;
    // 重ねがけは _remainingEnemyTurns で持続延長。Amount（粘液枚数）は Single のまま固定。
    public override PowerStackType StackType => PowerStackType.Single;

    public string? DisguiseName { get; private set; }

    /// <summary>
    /// true のとき意図ステートは触らず、PerformMove / PerformIntent を粘液付与に差し替える。
    /// </summary>
    public bool ShouldReplacePerform => _replacePerform && !_delivered;

    private SlimeDisguise.State? _disguise;
    private MoveState? _savedMove;
    private bool _restoredMove;
    private bool _replacePerform;
    private bool _delivered;
    private int _remainingEnemyTurns;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _remainingEnemyTurns = TurnScopedDuration.AddStack(_remainingEnemyTurns);

        TryApplyDisguise(applier);

        if (Owner == null || CombatState == null)
            return Task.CompletedTask;

        if (IntentOverwriteUnsafeMonsters.IsUnsafe(Owner))
        {
            _replacePerform = true;
            KaiserClubBackgroundSafety.StabilizeArm(Owner);
            SlimeDisguiseBanter.TryShowBlocked(applier, Owner);
            _ = SlimeIntentPresentation.TryRefreshAsync(Owner);
            return Task.CompletedTask;
        }

        if (!TryOverwriteIntent())
            _replacePerform = true;

        return Task.CompletedTask;
    }

    /// <summary>敵ターンの PerformMove 差し替え。未配信なら粘液を付与して完了とする。</summary>
    public async Task TryReplacePerformAsync()
    {
        if (_delivered || Owner == null || CombatState == null) return;
        _delivered = true;

        var count = Math.Max(1, Amount);
        var targets = CombatState.GetOpponentsOf(Owner).ToList();
        await ApplySlimeCardsAsync(CombatState, Owner, targets, count, silentPreview: true);
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

    /// <returns>意図上書きに成功したら true。</returns>
    private bool TryOverwriteIntent()
    {
        if (Owner?.Monster == null || CombatState == null) return false;
        if (IntentOverwriteUnsafeMonsters.IsUnsafe(Owner)) return false;

        var count = Math.Max(1, Amount);
        var combat = CombatState;
        var monster = Owner.Monster;
        var source = Owner;

        try
        {
            TryCaptureSavedMove(monster);

            async Task OnPerform(IReadOnlyList<Creature> targets)
            {
                // 敵ターン中のプレビュー待ち・Attack 待ちは進行不能の原因になりやすい（カイザー系と同型）
                await ApplySlimeCardsAsync(combat, source, targets, count, silentPreview: true);
            }

            var move = new MoveState(SlimeMoveId, OnPerform, [new StatusIntent(count)]);
            // 実行後に保存した行動へ戻す（FollowUp 未設定だとステートマシンが壊れる）
            if (_savedMove != null && !IsOurSlimeMove(_savedMove))
                move.FollowUpState = _savedMove;
            else
                move.FollowUpState = move;

            monster.SetMoveImmediate(move, forceTransition: true);
            return IsOurSlimeMove(monster.NextMove);
        }
        catch
        {
            // Intent API 差異時は PerformMove 差し替えへフォールバック
            return false;
        }
    }

    private static async Task ApplySlimeCardsAsync(
        ICombatState combat,
        Creature source,
        IReadOnlyList<Creature> targets,
        int count,
        bool silentPreview = false)
    {
        if (count <= 0 || targets.Count == 0) return;

        // 敵ターン差し替え（silentPreview）と不安全敵は Attack 待ちで進行不能になりやすい
        if (!silentPreview && !IntentOverwriteUnsafeMonsters.IsUnsafe(source))
        {
            try
            {
                await CreatureCmd.TriggerAnim(source, "Attack", SlimeAttackAnimDelay);
            }
            catch
            {
                // Attack アニメが無い敵でもカード付与は続行
            }
        }

        foreach (var target in targets)
        {
            var player = target.Player;
            if (player == null) continue;

            var cards = new List<CardModel>(count);
            for (var i = 0; i < count; i++)
            {
                // 通常の粘液。状態異常催眠中は StatusHypnosisPower のフックで AbnormalSlime に置換される。
                cards.Add(combat.CreateCard(ModelDb.Card<Slimed>(), player));
            }

            if (silentPreview)
            {
                await CombatCardPilePreview.AddGeneratedCardsSilentAsync(
                    cards, PileType.Discard, player);
            }
            else
            {
                await CombatCardPilePreview.AddGeneratedCardsAsync(
                    cards, PileType.Discard, player);
            }
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

        if (!TurnScopedDuration.Consume(ref _remainingEnemyTurns))
        {
            PrepareForNextEnemyTurn();
            return;
        }

        CleanupDisguise();
        TryRestoreSavedMove(Owner);
        await PowerCmd.Remove(this);
    }

    private void PrepareForNextEnemyTurn()
    {
        _delivered = false;
        _restoredMove = false;
        _savedMove = null;
        _replacePerform = false;

        if (Owner == null || CombatState == null) return;

        if (IntentOverwriteUnsafeMonsters.IsUnsafe(Owner))
        {
            _replacePerform = true;
            KaiserClubBackgroundSafety.StabilizeArm(Owner);
            _ = SlimeIntentPresentation.TryRefreshAsync(Owner);
            return;
        }

        if (!TryOverwriteIntent())
            _replacePerform = true;
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

        // PerformMove 差し替え経路／不安全モンスターはステートを触らない
        if (_replacePerform || IntentOverwriteUnsafeMonsters.IsUnsafe(creature))
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
