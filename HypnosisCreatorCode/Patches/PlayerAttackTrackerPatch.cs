using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>プレイヤーのアタックプレイ／ターン進行を記録する（急所の一刺し・ラポール等）。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class PlayerAttackTrackerCardPlayedPatch
{
    public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = combatState;
        _ = choiceContext;
        // CSV: 「アタックをプレイしていないターン」— 敵に当たったかではなくカード種別で判定
        if (cardPlay.Card.Type != CardType.Attack) return;

        var player = cardPlay.Card.Owner;
        if (player == null) return;

        var turn = player.PlayerCombatState?.TurnNumber ?? 0;
        PlayerAttackTracker.RecordAttack(player, turn);
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
