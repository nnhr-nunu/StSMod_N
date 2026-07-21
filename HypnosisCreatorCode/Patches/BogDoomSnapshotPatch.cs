using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 破滅の沼×1.5は「プレイ開始前に既にあった沼」のみ。
/// OnPlay 中に付与した沼が、同じカードのトランス／性癖刺さり破滅へ乗らないようにする。
/// 同一プレイで目覚めた性癖も、このスコープで刺さり判定から除外する。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
public static class BogDoomSnapshotBeforePatch
{
    public static void Prefix(ICombatState combatState, CardPlay play)
    {
        _ = play;
        FetishCombat.PushBogSnapshot(combatState);
        FetishCombat.PushAwakenPlayScope();
    }
}

/// <summary>
/// AfterCardPlayed の Prefix で沼スナップショットを外し、以降の反応（他色刺さり等）は
/// カードで付与済みの沼を通常どおり反映する。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class BogDoomSnapshotAfterPatch
{
    /// <summary>他の AfterCardPlayed より先に外し、カード付与後の沼を反応側へ見せる。</summary>
    [HarmonyPriority(Priority.First)]
    public static void Prefix(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay play)
    {
        _ = combatState;
        _ = choiceContext;
        _ = play;
        FetishCombat.PopBogSnapshot();
    }
}

/// <summary>
/// 目覚め除外スコープは AfterCardPlayed の刺さり解決（植え付け予約・他色）のあとで外す。
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class AwakenPlayScopeAfterPatch
{
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(ICombatState combatState, PlayerChoiceContext choiceContext, CardPlay play)
    {
        _ = combatState;
        _ = choiceContext;
        _ = play;
        FetishCombat.PopAwakenPlayScope();
    }
}
