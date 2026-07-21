using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HypnosisCreator.HypnosisCreatorCode.Utils;

/// <summary>
/// 状態異常催眠中、本家状態異常をプレイ可能版へ置き換える。
/// Slimed→粘液、Burn→火傷、Wound→負傷、Wither→衰微。
/// </summary>
public static class StatusHypnosisConvert
{
    private static int _reentry;

    /// <summary>本家状態異常 → プレイ可能版の canonical。</summary>
    public static CardModel? ResolvePlayableCanonical(CardModel card)
    {
        if (card is PlayableStatusCard) return null;

        return card switch
        {
            Slimed => ModelDb.Card<AbnormalSlime>(),
            Burn => ModelDb.Card<AbnormalBurn>(),
            Wound => ModelDb.Card<AbnormalWound>(),
            Wither => ModelDb.Card<AbnormalWither>(),
            _ => null
        };
    }

    public static bool OwnerHasStatusHypnosis(Player player) =>
        player.Creature?.GetPower<StatusHypnosisPower>() != null;

    /// <summary>戦闘中の手札・山札・捨て札・廃棄にある対象状態異常をすべて置き換える。</summary>
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
            var replacement = combat.CreateCard(canonical, player);
            // 催眠経由の置き換えは FreeEnemyPlay なし（トランス敵のみ）
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

        for (var i = 0; i < count; i++)
        {
            var card = combat.CreateCard(ModelDb.Card<T>(), player);
            if (card is PlayableStatusCard playable)
                playable.FreeEnemyPlay = freeEnemyPlay;
            await CardPileCmd.AddGeneratedCardToCombat(card, pile, player);
        }
    }
}
