using System.Reflection;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 脳くちゅ催眠 — 攻撃＋デバフ行動で、ダメージだけ他敵へ寄ったあとデバフがプレイヤーに残るのを防ぐ。
/// </summary>
[HarmonyPatch]
public static class BrainSlimeRedirectPowerModelApplyPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.DeclaredMethod(typeof(PowerCmd), nameof(PowerCmd.Apply),
        [
            typeof(PlayerChoiceContext), typeof(PowerModel), typeof(Creature),
            typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)
        ])!;

    public static void Prefix(Creature? applier, ref Creature target) =>
        BrainSlimeRedirectRules.TryRetarget(applier, ref target);
}
