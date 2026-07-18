using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>プレイヤーがアタックカードを敵にプレイしたターンを記録する。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class PlayerAttackTrackerPatch
{
    public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Card.Type != CardType.Attack) return;
        if (play.Target is not { IsEnemy: true }) return;

        var player = play.Card.Owner;
        var turn = player.PlayerCombatState?.TurnNumber ?? 0;
        PlayerAttackTracker.RecordAttack(player, turn);
    }
}
