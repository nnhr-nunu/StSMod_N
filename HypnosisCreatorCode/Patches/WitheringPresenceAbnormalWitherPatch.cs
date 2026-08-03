using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Powers;
using HarmonyLib;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 衰微（AbnormalWither）プレイ時は本家 WitheringPresence のカウントダウンを抑止し、
/// 全フック完了後に予兆を 6 へ戻す（使用後リセット）。
/// </summary>
[HarmonyPatch(typeof(WitheringPresencePower), nameof(WitheringPresencePower.AfterCardPlayed))]
public static class WitheringPresenceAbnormalWitherSkipPatch
{
    /// <summary>衰微プレイを「カード1枚」として数えない。</summary>
    public static bool Prefix(CardPlay cardPlay) => cardPlay.Card is not AbnormalWither;
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class AbnormalWitherOmenResetPatch
{
    public static void Postfix(ref Task __result, CardPlay cardPlay)
    {
        if (cardPlay.Card is not AbnormalWither) return;

        var target = cardPlay.Target;
        var owner = cardPlay.Card.Owner?.Creature;
        var original = __result;
        __result = Continue(original, target, owner);
    }

    private static async Task Continue(Task original, Creature? target, Creature? owner)
    {
        await original;
        WitherOmen.ResetOn(target);
        WitherOmen.ResetOn(owner);
    }
}
