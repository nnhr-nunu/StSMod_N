using MegaCrit.Sts2.Core.Combat;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 感覚共有 — このターン、単体アタックを全体化（UGは次ターン開始まで受けたダメージを敵全体へ伝播）。
/// 重ねがけで残り自分ターン+1。
/// </summary>
public class SenseSharePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    // 再入防止は await の流れごとに持つ（AsyncLocal）。ThreadStatic だと全プレイヤーで共有され、
    // 共闘で片方の波及が選択待ちで止まっている間、もう片方の感覚共有まで止まる。
    private static readonly AsyncLocal<bool> Resolving = new();

    private static readonly AsyncLocal<bool> Propagating = new();

    private int _remainingPlayerTurns;

    /// <summary>単体アタック全体化が有効な間 true。</summary>
    public bool AttackAoEActive { get; private set; }

    /// <summary>UG — 次の自ターン開始まで受けたダメージを敵全体へ伝播。</summary>
    public bool ReflectDamageActive { get; private set; }

    // 再適用は ModifyAmount に回り AfterApplied が呼ばれないため、初回・再適用とも AfterPowerAmountChanged で扱う。
    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (power != this || amount <= 0m) return Task.CompletedTask;

        if (AttackAoEActive)
            _remainingPlayerTurns = TurnScopedDuration.AddStack(_remainingPlayerTurns);
        else
        {
            AttackAoEActive = true;
            _remainingPlayerTurns = 1;
        }

        ReflectDamageActive |= cardSource is { IsUpgraded: true };
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!AttackAoEActive) return;
        if (Resolving.Value) return;
        if (Owner == null || CombatState == null) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (cardPlay.Card.Type != CardType.Attack) return;
        if (cardPlay.Card.TargetType != TargetType.AnyEnemy) return;
        if (cardPlay.Target is not { IsEnemy: true } primary) return;

        var player = cardPlay.Card.Owner;
        if (player == null) return;

        var others = CombatState.HittableEnemies.Where(e => e != primary && e.IsAlive).ToList();
        if (others.Count == 0) return;

        Resolving.Value = true;
        try
        {
            foreach (var enemy in others)
                await PropagatedCardPlay.OnEnemy(choiceContext, CombatState, cardPlay.Card, player, enemy);
        }
        finally
        {
            Resolving.Value = false;
        }
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (!ReflectDamageActive || target != Owner || CombatState == null) return;
        if (Propagating.Value || result.UnblockedDamage <= 0) return;

        var enemies = CombatState.HittableEnemies.Where(e => e.IsAlive && e.IsEnemy).ToList();
        if (enemies.Count == 0) return;

        Propagating.Value = true;
        try
        {
            foreach (var enemy in enemies)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    enemy,
                    result.UnblockedDamage,
                    ValueProp.Unpowered,
                    Owner,
                    cardSource,
                    null);
            }
        }
        finally
        {
            Propagating.Value = false;
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner)) return;

        if (TurnScopedDuration.Consume(ref _remainingPlayerTurns))
            AttackAoEActive = false;

        if (!AttackAoEActive && !ReflectDamageActive)
            await PowerCmd.Remove(this);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!ReflectDamageActive || Owner == null || player.Creature != Owner) return;

        ReflectDamageActive = false;
        if (!AttackAoEActive)
            await PowerCmd.Remove(this);
    }
}
