using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 戦闘中に生成カードを山に加えたあと、本家 <see cref="CardPileCmd.AddToCombatAndPreview{T}"/> 相当のプレビューを出す。
/// 手札は本家 <see cref="CardPileCmd.AddGeneratedCardsToCombat"/> をそのまま使う（プレビュー・skipVisuals なし）。
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

        var results = await CardPileCmd.AddGeneratedCardsToCombat(cards, pile, player, position);
        if (pile != PileType.Hand)
            await PreviewAddAsync(results, pile);
        return results;
    }

    public static async Task<CardPileAddResult> AddGeneratedCardAsync(
        CardModel card,
        PileType pile,
        Player player,
        CardPilePosition position = default)
    {
        var result = await CardPileCmd.AddGeneratedCardToCombat(card, pile, player, position);
        if (pile != PileType.Hand)
            await PreviewAddAsync([result], pile);
        return result;
    }

    /// <summary>追加のみ。プレビューは <see cref="PendingDrawPileCardPreview"/> 等で後追いする。</summary>
    public static Task<IReadOnlyList<CardPileAddResult>> AddGeneratedCardsSilentAsync(
        IReadOnlyList<CardModel> cards,
        PileType pile,
        Player player,
        CardPilePosition position = default)
    {
        if (cards.Count == 0)
            return Task.FromResult<IReadOnlyList<CardPileAddResult>>(Array.Empty<CardPileAddResult>());

        return CardPileCmd.AddGeneratedCardsToCombat(cards, pile, player, position);
    }

    /// <summary>
    /// 手札へ生成カードを追加。27db557 以前と同じ本家経路。
    /// skipVisuals や手動ノード生成は使わない（手札UI不整合・フリーズの原因）。
    /// </summary>
    public static Task<IReadOnlyList<CardPileAddResult>> AddToHandSkipVisualsAsync(
        IReadOnlyList<CardModel> cards,
        Player player,
        CardPilePosition position = default) =>
        AddGeneratedCardsSilentAsync(cards, PileType.Hand, player, position);

    public static async Task PreviewAddAsync(
        IReadOnlyList<CardPileAddResult> results,
        PileType pile)
    {
        if (results.Count == 0) return;

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
