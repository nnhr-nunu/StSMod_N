using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>性癖カードプレイ後: 植え付け予約の消費。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class FetishCardPlayedPatch
{
    public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Card is not HypnosisCreatorCard hc) return;
        if (hc.CardFetishes.Count == 0 && !hc.AlwaysHitsFetish) return;

        // fire-and-forget ではなく同期待ちが必要なら後で Task パッチに変更
        FetishPlantPending.TryConsumeOnPlay(hc.Owner, play.Target, hc.CardFetishes)
            .GetAwaiter()
            .GetResult();
    }
}
