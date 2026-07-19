using HypnosisCreator.HypnosisCreatorCode.CustomEnums;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>カウントのプレイ可否。mechanics-lock.md 参照。</summary>
public static class CountRules
{
    public static bool HasCountKeyword(CardModel card) =>
        card.Keywords.Contains(HcKeywords.Count);

    public static bool IsResolvedCostZero(CardModel card) =>
        card.EnergyCost.GetResolved() == 0;

    /// <summary>手札から掴めるか（単体は「誰かがトランス」で可）。</summary>
    public static bool CanStartPlay(CardModel card)
    {
        if (!HasCountKeyword(card)) return true;
        if (IsResolvedCostZero(card)) return true;

        var combat = card.CombatState;
        if (combat == null) return false;

        return card.TargetType switch
        {
            TargetType.AllEnemies or TargetType.RandomEnemy =>
                TranceCombat.AnyEnemyHasTrance(combat.HittableEnemies),
            TargetType.AnyEnemy =>
                TranceCombat.AnyEnemyHasTrance(combat.HittableEnemies),
            _ => false // Self / None 等: トランス解除なし
        };
    }

    /// <summary>着弾対象が有効か。</summary>
    public static bool CanPlayTargeting(CardModel card, Creature target)
    {
        if (!HasCountKeyword(card)) return true;
        if (IsResolvedCostZero(card)) return true;

        return card.TargetType switch
        {
            TargetType.AnyEnemy => TranceCombat.HasTrance(target),
            TargetType.AllEnemies or TargetType.RandomEnemy =>
                card.CombatState != null &&
                TranceCombat.AnyEnemyHasTrance(card.CombatState.HittableEnemies),
            _ => false
        };
    }

    /// <summary>手札のカウントカードのコストを steps だけ下げる（ラポール・指折り数えて等）。</summary>
    public static void AdvanceHandCountCards(Player player, int steps = 1)
    {
        if (steps <= 0) return;
        var hand = player.PlayerCombatState?.Hand;
        if (hand == null) return;

        foreach (var card in hand.Cards.ToList())
        {
            if (!HasCountKeyword(card)) continue;
            if (card.EnergyCost.GetResolved() <= 0) continue;
            card.EnergyCost.AddThisCombat(-steps);
        }
    }

    /// <summary>手札の催眠系カウントカードのコストを0にする。</summary>
    public static void ZeroHandCountCosts(Player player)
    {
        var hand = player.PlayerCombatState?.Hand;
        if (hand == null) return;

        foreach (var card in hand.Cards.ToList())
        {
            if (!HasCountKeyword(card)) continue;
            var resolved = card.EnergyCost.GetResolved();
            if (resolved <= 0) continue;
            card.EnergyCost.AddThisCombat(-resolved);
        }
    }

    /// <summary>
    /// 手札のカウントカードの解決後コスト合計。
    /// <paramref name="exclude"/> は自分自身（期待に応えて等）を除外するときに使う。
    /// </summary>
    public static int SumResolvedCountCostsInHand(Player player, CardModel? exclude = null)
    {
        var hand = player.PlayerCombatState?.Hand;
        if (hand == null) return 0;

        var sum = 0;
        foreach (var card in hand.Cards)
        {
            if (exclude != null && ReferenceEquals(card, exclude)) continue;
            if (!HasCountKeyword(card)) continue;
            var resolved = card.EnergyCost.GetResolved();
            if (resolved > 0) sum += resolved;
        }
        return sum;
    }

    /// <summary>手札のカウントカード一覧（コスト低下処理用）。</summary>
    public static List<CardModel> GetCountCardsInHand(Player player, CardModel? exclude = null)
    {
        var hand = player.PlayerCombatState?.Hand;
        if (hand == null) return [];

        return hand.Cards
            .Where(c => (exclude == null || !ReferenceEquals(c, exclude)) && HasCountKeyword(c))
            .ToList();
    }
}
