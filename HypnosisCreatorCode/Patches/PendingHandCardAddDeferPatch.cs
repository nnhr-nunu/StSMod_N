using System.Linq;
using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// カードプレイ中の手札追加を <see cref="PendingHandCardAdd"/> へ一括委譲する。
/// 個別カードで経路を揃え忘れても、飛来アニメ競合による固まりを防ぐ。
/// </summary>
[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.AddGeneratedCardsToCombat))]
public static class PendingHandCardAddGeneratedDeferPatch
{
    public static bool Prefix(
        IEnumerable<CardModel> cards,
        PileType newPileType,
        Player? creator,
        ref Task<IReadOnlyList<CardPileAddResult>> __result)
    {
        if (newPileType != PileType.Hand || PendingHandCardAdd.IsFlushing)
            return true;

        var list = cards.ToList();
        if (list.Count == 0)
            return true;

        var player = creator ?? list[0].Owner;
        if (player == null || !CombatManager.Instance.IsExecutingCardOrPotionEffect(player))
            return true;

        PendingHandCardAdd.EnqueueGenerated(list, player);
        __result = Task.FromResult<IReadOnlyList<CardPileAddResult>>(
            list.Select(c => new CardPileAddResult { success = true, cardAdded = c }).ToList());
        return false;
    }
}

/// <summary>既存カードの手札移動（演出あり）もプレイ中は遅延する。</summary>
[HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add),
    typeof(CardModel), typeof(PileType), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool))]
public static class PendingHandCardAddExistingDeferPatch
{
    public static bool Prefix(
        CardModel card,
        PileType newPileType,
        CardPilePosition position,
        AbstractModel? clonedBy,
        bool skipVisuals,
        ref Task<CardPileAddResult> __result)
    {
        if (newPileType != PileType.Hand || skipVisuals || PendingHandCardAdd.IsFlushing)
            return true;

        var player = card.Owner;
        if (player == null || !CombatManager.Instance.IsExecutingCardOrPotionEffect(player))
            return true;

        PendingHandCardAdd.EnqueueExisting([card], player, clonedBy, position);
        __result = Task.FromResult(new CardPileAddResult { success = true, cardAdded = card });
        return false;
    }
}
