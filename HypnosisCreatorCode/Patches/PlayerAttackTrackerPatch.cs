using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>プレイヤーのアタックプレイ／ターン進行を記録する（急所の一刺し・ラポール等）。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class PlayerAttackTrackerCardPlayedPatch
{
    public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay play)
    {
        _ = choiceContext;
        if (play.Card.Type != CardType.Attack) return;
        if (!TargetsEnemy(play, combatState)) return;

        var player = play.Card.Owner;
        if (player == null) return;

        var turn = player.PlayerCombatState?.TurnNumber ?? 0;
        PlayerAttackTracker.RecordAttack(player, turn);
    }

    private static bool TargetsEnemy(CardPlay play, ICombatState combatState)
    {
        if (play.Target is { IsEnemy: true }) return true;

        return play.Card.TargetType switch
        {
            TargetType.AllEnemies or TargetType.RandomEnemy => true,
            _ => false
        };
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
public static class PlayerAttackTrackerTurnStartPatch
{
    public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, Player player)
    {
        _ = combatState;
        _ = choiceContext;
        PlayerAttackTracker.BeginPlayerTurn(player);
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeSideTurnEnd))]
public static class PlayerAttackTrackerTurnEndPatch
{
    public static void Postfix(
        ICombatState combatState,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        _ = combatState;
        if (side != CombatSide.Player) return;

        foreach (var creature in participants)
        {
            var player = creature.Player;
            if (player != null)
                PlayerAttackTracker.FinalizePlayerTurn(player);
        }
    }
}
