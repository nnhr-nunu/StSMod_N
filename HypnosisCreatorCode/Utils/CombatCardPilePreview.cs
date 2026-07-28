using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 戦闘中に生成カードを山に加えたあと、本家 <see cref="CardPileCmd.AddToCombatAndPreview{T}"/> 相当のプレビューを出す。
/// ターン開始の手札追加は <see cref="CardPileCmd.AddGeneratedCardsToCombat"/> を即時実行。
/// カードプレイ中の手札追加は <see cref="PendingHandCardAdd"/> でプレイ完了後に実行する。
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

        if (pile == PileType.Hand && CombatManager.Instance.IsExecutingCardOrPotionEffect(player))
        {
            await AddToHandDuringCardPlayAsync(cards, player);
            return Array.Empty<CardPileAddResult>();
        }

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
    /// カードプレイ中の手札へ生成カードを追加（調教コマンド等）。
    /// 飛来アニメがプレイ演出と競合して固まるため、<see cref="Hook.AfterCardPlayed"/> 後に追加する。
    /// </summary>
    public static Task AddToHandDuringCardPlayAsync(
        IReadOnlyList<CardModel> cards,
        Player player)
    {
        PendingHandCardAdd.EnqueueGenerated(cards, player);
        return Task.CompletedTask;
    }

    /// <summary>即時手札追加のエイリアス（カードプレイ中は遅延キューへ）。</summary>
    public static Task AddToHandSkipVisualsAsync(
        IReadOnlyList<CardModel> cards,
        Player player,
        CardPilePosition position = default)
    {
        if (CombatManager.Instance.IsExecutingCardOrPotionEffect(player))
            return AddToHandDuringCardPlayAsync(cards, player);

        return CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, player, position);
    }

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

/// <summary>
/// カードプレイ中に手札へ入れるカードをキューし、プレイ完了後にまとめて追加する。
/// </summary>
public static class PendingHandCardAdd
{
    private sealed class GeneratedBatch
    {
        public required List<CardModel> Cards { get; init; }
        public required Player Player { get; init; }
    }

    private sealed class ExistingMove
    {
        public required CardModel Card { get; init; }
        public required Player Player { get; init; }
        public AbstractModel? Source { get; init; }
        public CardPilePosition Position { get; init; }
    }

    private static readonly List<GeneratedBatch> GeneratedQueue = [];
    private static readonly List<ExistingMove> ExistingQueue = [];

    public static void EnqueueGenerated(IReadOnlyList<CardModel> cards, Player player)
    {
        if (cards.Count == 0) return;
        GeneratedQueue.Add(new GeneratedBatch
        {
            Cards = cards.ToList(),
            Player = player
        });
    }

    public static void EnqueueExisting(
        IReadOnlyList<CardModel> cards,
        Player player,
        AbstractModel? source = null,
        CardPilePosition position = default)
    {
        foreach (var card in cards)
            ExistingQueue.Add(new ExistingMove
            {
                Card = card,
                Player = player,
                Source = source,
                Position = position
            });
    }

    public static async Task FlushIfAnyAsync()
    {
        if (GeneratedQueue.Count == 0 && ExistingQueue.Count == 0) return;

        var generated = GeneratedQueue.ToList();
        var existing = ExistingQueue.ToList();
        GeneratedQueue.Clear();
        ExistingQueue.Clear();

        foreach (var move in existing)
            await CardPileCmd.Add(
                move.Card, PileType.Hand, move.Position, move.Source, skipVisuals: false);

        foreach (var batch in generated)
            await CardPileCmd.AddGeneratedCardsToCombat(
                batch.Cards, PileType.Hand, batch.Player);
    }

    public static void Clear()
    {
        GeneratedQueue.Clear();
        ExistingQueue.Clear();
    }
}
