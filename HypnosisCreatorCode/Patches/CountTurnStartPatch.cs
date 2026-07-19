using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// プレイヤーターン開始の手札ドロー前: すでに手札にあるカウント（保留など）のコストを1下げる。
/// ドロー後に下げると、今引いたカードまで即−1されてしまうため
/// <see cref="Hook.BeforeHandDraw"/> で処理する（過去に動作実績あり）。
/// private TargetMethod + throw は PatchAll を中断するため使わない。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeHandDraw))]
public static class CountTurnStartPatch
{
    public static void Postfix(ICombatState combatState, Player player, PlayerChoiceContext playerChoiceContext)
    {
        CountRules.AdvanceHandCountCards(player);
    }
}
