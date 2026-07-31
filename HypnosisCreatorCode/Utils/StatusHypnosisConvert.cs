using System.Linq;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 状態異常催眠中、本家状態異常／呪いをプレイ可能版へ置き換える。
/// </summary>
public static class StatusHypnosisConvert
{
    private static int _reentry;

    /// <summary>本家状態異常／呪い → プレイ可能版の canonical。</summary>
    public static CardModel? ResolvePlayableCanonical(CardModel card)
    {
        if (card is PlayableStatusCard or PlayableCurseCard) return null;

        return card switch
        {
            // Status
            Slimed => ModelDb.Card<AbnormalSlime>(),
            Burn => ModelDb.Card<AbnormalBurn>(),
            Wound => ModelDb.Card<AbnormalWound>(),
            Wither => ModelDb.Card<AbnormalWither>(),
            Soot => ModelDb.Card<SootStatus>(),
            Dazed => ModelDb.Card<DazedStatus>(),
            Infection => ModelDb.Card<Infect>(),
            MegaCrit.Sts2.Core.Models.Cards.Void => ModelDb.Card<VoidStatus>(),
            Debris => ModelDb.Card<DebrisStatus>(),
            FranticEscape => ModelDb.Card<FranticEscapeStatus>(),
            Toxic => ModelDb.Card<ToxicStatus>(),
            Beckon => ModelDb.Card<BeckonStatus>(),
            // Curse
            AscendersBane => ModelDb.Card<AscendersBaneCurse>(),
            Injury => ModelDb.Card<InjuryCurse>(),
            Greed => ModelDb.Card<GreedCurse>(),
            Doubt => ModelDb.Card<DoubtCurse>(),
            Writhe => ModelDb.Card<WritheCurse>(),
            Folly => ModelDb.Card<FollyCurse>(),
            Regret => ModelDb.Card<RegretCurse>(),
            Guilty => ModelDb.Card<GuiltyCurse>(),
            CurseOfTheBell => ModelDb.Card<CurseOfTheBellCurse>(),
            PoorSleep => ModelDb.Card<PoorSleepCurse>(),
            BadLuck => ModelDb.Card<BadLuckCurse>(),
            Clumsy => ModelDb.Card<ClumsyCurse>(),
            Decay => ModelDb.Card<DecayCurse>(),
            Debt => ModelDb.Card<DebtCurse>(),
            Normality => ModelDb.Card<NormalityCurse>(),
            Shame => ModelDb.Card<ShameCurse>(),
            SporeMind => ModelDb.Card<SporeMindCurse>(),
            Enthralled => ModelDb.Card<EnthralledCurse>(),
            _ => null
        };
    }

    /// <summary>状態異常催眠のプレイ可能呪いではなく、本家 <see cref="CardType.Curse"/> のみ。</summary>
    public static bool IsVanillaCurseCard(CardModel card) =>
        card.Type == CardType.Curse && card is not PlayableCurseCard;

    public static bool OwnerHasStatusHypnosis(Player player) =>
        player.Creature?.GetPower<StatusHypnosisPower>() != null;

    /// <summary>戦闘中の手札・山札・捨て札・廃棄にある対象をすべて置き換える。</summary>
    public static async Task ConvertAllCombatStatuses(Player player)
    {
        if (!OwnerHasStatusHypnosis(player)) return;
        var combat = player.PlayerCombatState;
        if (combat == null) return;

        foreach (var pile in new[] { combat.Hand, combat.DrawPile, combat.DiscardPile, combat.ExhaustPile })
        {
            if (pile == null) continue;
            foreach (var card in pile.Cards.ToList())
                await TryConvertAsync(card, player);
        }
    }

    /// <summary>1枚を置き換え。既にプレイ可能版／対応外なら何もしない。</summary>
    public static async Task TryConvertAsync(CardModel card, Player player)
    {
        if (_reentry > 0) return;
        if (!OwnerHasStatusHypnosis(player)) return;
        if (card.Owner != player) return;

        var canonical = ResolvePlayableCanonical(card);
        if (canonical == null) return;

        var combat = player.Creature?.CombatState;
        if (combat == null) return;

        _reentry++;
        try
        {
            // 永劫付き呪い（災厄・強欲・愚行・鐘の呪い・魅了）は本家 IsTransformable=false のため、
            // 一時除去してから置換する（プレイ可能版側の CanonicalKeywords で再付与される）。
            if (card.Keywords.Contains(CardKeyword.Eternal))
                card.RemoveKeyword(CardKeyword.Eternal);

            if (!card.IsTransformable) return;

            var replacement = combat.CreateCard(canonical, player);
            await CardCmd.Transform(card, replacement, CardPreviewStyle.None);
        }
        finally
        {
            _reentry--;
        }
    }

    /// <summary>心臓用：プレイ可能状態異常を指定枚数・指定山へ（敵へプレイ可）。</summary>
    public static async Task AddFreePlayableAsync<T>(Player player, int count, PileType pile)
        where T : PlayableStatusCard
        => await AddPlayableStatusAsync<T>(player, count, pile, freeEnemyPlay: true);

    /// <summary>
    /// 心臓用：状態異常を指定枚数・指定山へ。
    /// <paramref name="freeEnemyPlay"/> は敵対象カード用。自分対象（感染など）は false。
    /// </summary>
    public static async Task AddPlayableStatusAsync<T>(
        Player player, int count, PileType pile, bool freeEnemyPlay = false)
        where T : PlayableStatusCard
    {
        var combat = player.Creature?.CombatState;
        if (combat == null || count <= 0) return;

        var cards = new List<CardModel>(count);
        for (var i = 0; i < count; i++)
        {
            var card = combat.CreateCard(ModelDb.Card<T>(), player);
            if (card is PlayableStatusCard playable)
                playable.FreeEnemyPlay = freeEnemyPlay;
            cards.Add(card);
        }

        if (pile == PileType.Hand)
        {
            await AddGeneratedToHandAsync(cards, player);
            return;
        }

        var results = await CardPileCmd.AddGeneratedCardsToCombat(
            cards, pile, player, CombatCardPilePreview.DefaultPositionFor(pile));
        await CombatCardPilePreview.PreviewAddAsync(results, pile);
    }

    /// <summary>
    /// 手札への生成カード追加。プレイ中は遅延キュー、それ以外は1枚ずつ追加（複数枚バッチは飛来演出が止まる）。
    /// </summary>
    public static async Task AddGeneratedToHandAsync(IReadOnlyList<CardModel> cards, Player player)
    {
        if (cards.Count == 0) return;

        if (CombatManager.Instance.IsExecutingCardOrPotionEffect(player))
        {
            await CombatCardPilePreview.AddToHandDuringCardPlayAsync(cards, player);
            return;
        }

        foreach (var card in cards)
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        await Cmd.Wait(0.1f);
    }

    /// <summary>
    /// 生成カードの山札追加後、本家 <see cref="CardPileCmd.AddToCombatAndPreview{T}"/> 同様のプレビューを出す。
    /// 捨て札など画面外の山へ入れるとき、無反応に見えないようにする。
    /// </summary>
    public static Task PreviewGeneratedPileAddAsync(
        IReadOnlyList<CardPileAddResult> results,
        PileType pile) =>
        CombatCardPilePreview.PreviewAddAsync(results, pile);
}

/// <summary>
/// 生成直後の Transform は本家の捨て札追加演出・プレビューを壊すため、演出後にまとめて変換する。
/// </summary>
public static class PendingStatusHypnosisConvert
{
    private static readonly List<(CardModel Card, Player Player)> Queue = [];

    public static void Enqueue(CardModel card, Player player)
    {
        if (Queue.Any(e => e.Card == card)) return;
        Queue.Add((card, player));
    }

    public static async Task FlushIfAnyAsync()
    {
        if (Queue.Count == 0) return;
        var batch = Queue.ToList();
        Queue.Clear();
        foreach (var (card, player) in batch)
            await StatusHypnosisConvert.TryConvertAsync(card, player);
    }

    public static void Clear() => Queue.Clear();
}
