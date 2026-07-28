using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 戦闘中に生成カードを山に加えたあと、本家 <see cref="CardPileCmd.AddToCombatAndPreview{T}"/> 相当のプレビューを出す。
/// 手札追加は飛来アニメを使わず <see cref="AddToHandSkipVisualsAsync"/> に統一（進行不能防止）。
/// </summary>
public static class CombatCardPilePreview
{
    public static async Task<IReadOnlyList<CardPileAddResult>> AddGeneratedCardsAsync(
        IReadOnlyList<CardModel> cards,
        PileType pile,
        Player player,
        CardPilePosition position = default)
    {
        if (cards.Count == 0)
            return Array.Empty<CardPileAddResult>();

        if (pile == PileType.Hand)
            return await AddToHandSkipVisualsAsync(cards, player, position: position);

        var results = await CardPileCmd.AddGeneratedCardsToCombat(cards, pile, player, position);
        await PreviewAddAsync(results, pile);
        return results;
    }

    public static async Task<CardPileAddResult> AddGeneratedCardAsync(
        CardModel card,
        PileType pile,
        Player player,
        CardPilePosition position = default)
    {
        if (pile == PileType.Hand)
        {
            var results = await AddToHandSkipVisualsAsync([card], player, position: position);
            return results[0];
        }

        var result = await CardPileCmd.AddGeneratedCardToCombat(card, pile, player, position);
        await PreviewAddAsync([result], pile);
        return result;
    }

    /// <summary>追加のみ。プレビューは <see cref="PendingDrawPileCardPreview"/> 等で後追いする。</summary>
    public static async Task<IReadOnlyList<CardPileAddResult>> AddGeneratedCardsSilentAsync(
        IReadOnlyList<CardModel> cards,
        PileType pile,
        Player player,
        CardPilePosition position = default)
    {
        if (cards.Count == 0)
            return Array.Empty<CardPileAddResult>();

        if (pile == PileType.Hand)
            return await AddToHandSkipVisualsAsync(cards, player, position: position);

        return await CardPileCmd.AddGeneratedCardsToCombat(cards, pile, player, position);
    }

    /// <summary>
    /// 手札へ即時追加（飛来アニメなし）。本家 <see cref="CardPileCmd.AddGeneratedCardsToCombat"/> と同じ
    /// 履歴登録・フック順序で、<see cref="CardPileCmd.Add"/> だけ <c>skipVisuals: true</c> にする。
    /// <paramref name="invokeDrawHooks"/> でスネッコアイ等の <see cref="Hook.AfterCardDrawn"/> を後追いする。
    /// </summary>
    public static async Task<IReadOnlyList<CardPileAddResult>> AddToHandSkipVisualsAsync(
        IReadOnlyList<CardModel> cards,
        Player player,
        AbstractModel? clonedBy = null,
        CardPilePosition position = default,
        PlayerChoiceContext? choiceContext = null,
        bool invokeDrawHooks = false)
    {
        if (cards.Count == 0)
            return Array.Empty<CardPileAddResult>();

        if (!CombatManager.Instance.IsInProgress)
            return Array.Empty<CardPileAddResult>();

        if (cards.Any(c => c.Pile != null))
            throw new InvalidOperationException(
                "Generated cards must not already be in a pile before hand add.");

        var combat = player.Creature?.CombatState;
        if (combat == null)
            return Array.Empty<CardPileAddResult>();

        var handPile = PileType.Hand.GetPile(player);
        var results = new List<CardPileAddResult>(cards.Count);

        foreach (var card in cards)
        {
            CombatManager.Instance.History.CardGenerated(combat, card, player);

            var result = await CardPileCmd.Add(
                card, handPile, position, clonedBy, skipVisuals: true);
            results.Add(result);

            await Hook.AfterCardGeneratedForCombat(combat, card, player);

            if (invokeDrawHooks && choiceContext != null)
                await Hook.AfterCardDrawn(combat, choiceContext, card, fromHandDraw: false);
        }

        return results;
    }

    public static async Task PreviewAddAsync(
        IReadOnlyList<CardPileAddResult> results,
        PileType pile)
    {
        if (results.Count == 0) return;

        // 手札は追加後にそのまま見える。山札・捨て札のみプレビュー。
        if (pile == PileType.Hand)
            return;

        var style = results.Count <= 5
            ? CardPreviewStyle.HorizontalLayout
            : CardPreviewStyle.MessyLayout;
        CardCmd.PreviewCardPileAdd(results, 1.2f, style);
        await Cmd.Wait(1f);
    }
}

/// <summary>
/// 攻撃ヒット中に山札へ入るカード（エントマンサーのめまい等）を、カードプレイ完了後にまとめてプレビューする。
/// 多段攻撃中は本家プレビューが埋もれやすいため遅延表示する。
/// </summary>
public static class PendingDrawPileCardPreview
{
    private static readonly List<CardPileAddResult> Queue = [];

    public static void Enqueue(IReadOnlyList<CardPileAddResult> results)
    {
        foreach (var result in results)
            Queue.Add(result);
    }

    public static async Task FlushIfAnyAsync()
    {
        if (Queue.Count == 0) return;

        var batch = Queue.ToList();
        Queue.Clear();
        await CombatCardPilePreview.PreviewAddAsync(batch, PileType.Draw);
    }

    public static void Clear() => Queue.Clear();
}
