using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>プレイヤーターン開始: 手札のカウントカードのコストを1下げる。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
public static class CountTurnStartPatch
{
    public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, Player player)
    {
        var hand = player.PlayerCombatState?.Hand;
        if (hand == null) return;

        foreach (var card in hand.Cards.ToList())
        {
            if (!card.Keywords.Contains(HcKeywords.Count)) continue;
            if (card.EnergyCost.GetResolved() <= 0) continue;
            card.EnergyCost.AddThisCombat(-1);
        }
    }
}
