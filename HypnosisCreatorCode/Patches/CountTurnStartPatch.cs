using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// プレイヤーターン開始・手札ドロー前に、すでに手札にあるカウント（保留）のコストを1下げる。
/// private の <c>CombatManager.SetupPlayerTurn</c> 先頭で処理する
///（BeforeHandDraw → Draw より前なので、今引くカードは下がらない）。
/// </summary>
[HarmonyPatch]
public static class CountTurnStartPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.Method(
            typeof(CombatManager),
            "SetupPlayerTurn",
            [typeof(Player), typeof(HookPlayerChoiceContext)])
        ?? throw new InvalidOperationException("CombatManager.SetupPlayerTurn not found");

    public static void Prefix(Player player)
    {
        CountRules.AdvanceHandCountCards(player);
    }
}
