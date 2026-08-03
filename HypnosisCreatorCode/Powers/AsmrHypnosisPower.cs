using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// ASMR催眠 — マルチではプレイヤーを Left/Right に均等割り当てし、左右交互のプレイで破滅を付与する。
/// ソロでは自己交互（1プレイおき）で同様に動作する。
/// 敵には最後に攻撃した担当側を AsmrLeftPower / AsmrRightPower で表示する。
/// </summary>
public class AsmrHypnosisPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private bool? _lastWasLeft;

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (CombatState == null || applier?.Player == null) return;

        var ctx = new ThrowingPlayerChoiceContext();
        var players = CombatState.Players.ToList();
        var rng = applier.Player.RunState.Rng.CombatCardSelection;
        await AsmrSideRules.AssignPlayersAsync(ctx, players, rng, applier);

        foreach (var enemy in CombatState.HittableEnemies)
        {
            if (enemy.GetPower<AsmrEnemySideTrackerPower>() != null) continue;
            await PowerCmd.Apply<AsmrEnemySideTrackerPower>(
                ctx, enemy, 1M, applier, cardSource, silent: true);
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || CombatState == null) return;
        var player = cardPlay.Card.Owner;
        if (player == null) return;
        if (!CombatState.Players.Contains(player)) return;

        if (!AsmrSideRules.IsAssigned && CombatState.Players.Count > 1)
        {
            var rng = player.RunState.Rng.CombatCardSelection;
            await AsmrSideRules.AssignPlayersAsync(
                choiceContext, CombatState.Players.ToList(), rng, Owner);
        }

        var isLeft = AsmrSideRules.ResolvePlaySide(player, _lastWasLeft);

        if (cardPlay.Target is { IsEnemy: true } hitEnemy)
            await AsmrSideRules.SyncEnemyHitSideAsync(choiceContext, hitEnemy, isLeft, player.Creature);

        if (_lastWasLeft is null)
        {
            _lastWasLeft = isLeft;
            return;
        }

        if (_lastWasLeft == isLeft) return;

        var doomTarget = ResolveDoomTarget(cardPlay, player);
        if (doomTarget != null)
            await FetishCombat.ApplyDoom(choiceContext, doomTarget, Amount, Owner, cardPlay.Card);

        _lastWasLeft = isLeft;
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await ClearSideMarkers(oldOwner);
        await base.AfterRemoved(oldOwner);
    }

    private Creature? ResolveDoomTarget(CardPlay cardPlay, Player player)
    {
        if (CombatState == null) return null;

        if (cardPlay.Target is { IsEnemy: true, IsAlive: true } target)
            return target;

        var marked = CombatState.HittableEnemies
            .Where(e => e.GetPower<AsmrLeftPower>() != null || e.GetPower<AsmrRightPower>() != null)
            .ToList();
        if (marked.Count > 0)
        {
            var rng = player.RunState.Rng.CombatCardSelection;
            return marked[rng.NextInt(marked.Count)];
        }

        var enemies = CombatState.HittableEnemies.ToList();
        if (enemies.Count == 0) return null;

        var fallbackRng = player.RunState.Rng.CombatCardSelection;
        return enemies[fallbackRng.NextInt(enemies.Count)];
    }

    private static async Task ClearSideMarkers(Creature owner)
    {
        var left = owner.GetPower<AsmrLeftPower>();
        if (left != null) await PowerCmd.Remove(left);
        var right = owner.GetPower<AsmrRightPower>();
        if (right != null) await PowerCmd.Remove(right);
    }
}
