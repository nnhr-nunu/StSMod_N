using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// 性癖タグ（SM／DomSub／アブノーマル／トランス性癖）と、説明文の金文字トランス／破滅／沼に
/// キーワードのツールチップを付ける。ツールチップ過多時は <see cref="HoverTipCrowding"/> の条件で省略する。
/// <para>
/// ホバー表示（<see cref="CardModel.HoverTips"/>）にだけ足し、<see cref="CardModel.Keywords"/> には足さない。
/// Keywords は共闘の同期チェック（本家 NetFullCombatState.CardState の keywords）に入るため、表示言語や
/// ホバー順で埋まるキャッシュに左右される値を混ぜると、端末ごとに差が出て同期失敗になる。
/// </para>
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.HoverTips), MethodType.Getter)]
public static class MechanicKeywordPatch
{
    public static void Postfix(CardModel __instance, ref IEnumerable<IHoverTip> __result)
    {
        KeywordPatchGuard.Enter();
        try
        {
            if (CardLibraryUiGuard.IsActive || __instance.IsCanonical) return;

            var owned = __instance.Keywords;
            var extra = DisplayKeywords(__instance)
                .Where(kw => !owned.Contains(kw))
                .Distinct()
                .ToList();
            if (extra.Count == 0) return;

            __result = __result
                .Concat(extra.Select(HoverTipFactory.FromKeyword))
                .Distinct()
                .ToList();
        }
        finally
        {
            KeywordPatchGuard.Leave();
        }
    }

    private static IEnumerable<CardKeyword> DisplayKeywords(CardModel card)
    {
        var fetish = FetishCardText.KeywordsFor(card).ToList();
        if (HoverTipCrowding.ShouldOmitStarterFetishKeywordTips(card))
            fetish.RemoveAll(HoverTipCrowding.IsStarterFetishKeyword);
        foreach (var kw in fetish)
            yield return kw;

        if (HoverTipCrowding.IsCrowded(card))
            yield break;
        foreach (var kw in MechanicKeywordRules.KeywordsFor(card))
            yield return kw;
    }
}
