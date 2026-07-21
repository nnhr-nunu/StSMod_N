using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using System.Threading.Tasks;
using HarmonyLib;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 衰微プレイ後、本家の衰微の予兆カウントを6に戻す（使用後リセット）。
/// </summary>
[HarmonyPatch(typeof(WitheringPresencePower), nameof(WitheringPresencePower.AfterCardPlayed))]
public static class WitheringPresenceAbnormalWitherPatch
{
    public static void Postfix(WitheringPresencePower __instance, ref Task __result, CardPlay cardPlay)
    {
        if (cardPlay.Card is not AbnormalWither) return;
        var original = __result;
        __result = Continue(original, __instance);
    }

    private static async Task Continue(Task original, WitheringPresencePower power)
    {
        await original;
        WitherOmen.ResetPower(power);
    }
}
