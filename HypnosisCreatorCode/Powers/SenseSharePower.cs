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
/// </summary>
public class SenseSharePower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    [ThreadStatic]
    private static bool _resolving;

    [ThreadStatic]
    private static bool _propagating;

    /// <summary>このターン中の単体アタック全体化。ターン終了で無効。</summary>
    public bool AttackAoEActive { get; private set; }

    /// <summary>UG — 次の自ターン開始まで受けたダメージを敵全体へ伝播。</summary>
    public bool ReflectDamageActive { get; private set; }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        AttackAoEActive = true;
        ReflectDamageActive = cardSource is { IsUpgraded: true };
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!AttackAoEActive) return;
        if (_resolving) return;
        if (Owner == null || CombatState == null) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (cardPlay.Card.Type != CardType.Attack) return;
        if (cardPlay.Card.TargetType != TargetType.AnyEnemy) return;
        if (cardPlay.Target is not { IsEnemy: true } primary) return;

        var player = cardPlay.Card.Owner;
        if (player == null) return;

        var others = CombatState.HittableEnemies.Where(e => e != primary && e.IsAlive).ToList();
        if (others.Count == 0) return;

        _resolving = true;
        try
        {
            var canonical = cardPlay.Card.CanonicalInstance ?? cardPlay.Card;
            foreach (var enemy in others)
                await PropagatedCardPlay.OnEnemy(choiceContext, CombatState, canonical, player, enemy);
        }
        finally
        {
            _resolving = false;
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
        if (_propagating || result.UnblockedDamage <= 0) return;

        var enemies = CombatState.HittableEnemies.Where(e => e.IsAlive && e.IsEnemy).ToList();
        if (enemies.Count == 0) return;

        _propagating = true;
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
            _propagating = false;
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

        AttackAoEActive = false;
        if (!ReflectDamageActive)
            await PowerCmd.Remove(this);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!ReflectDamageActive || Owner == null || player.Creature != Owner) return;

        ReflectDamageActive = false;
        await PowerCmd.Remove(this);
    }
}
