using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// プレイヤーターン開始の手札ドロー前: すでに手札にあるカウント（保留など）のコストを1下げる。
/// ドロー後に下げると、今引いたカードまで即−1されてしまう（開幕3コストが2になる等）ため、
/// <see cref="Hook.BeforeHandDraw"/> で処理する。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeHandDraw))]
public static class CountTurnStartPatch
{
    public static void Postfix(ICombatState combatState, Player player, PlayerChoiceContext playerChoiceContext)
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
