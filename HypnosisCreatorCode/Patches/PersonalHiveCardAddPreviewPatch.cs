using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>
/// エントマンサー（Personal Hive）— めまいの山札追加を攻撃カード完了後にまとめてプレビューする。
/// </summary>
[HarmonyPatch(typeof(PersonalHivePower), nameof(PersonalHivePower.AfterDamageReceived))]
public static class PersonalHiveCardAddPreviewPatch
{
    public static bool Prefix(
        PersonalHivePower __instance,
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        ref Task __result)
    {
        _ = choiceContext;
        _ = result;
        _ = cardSource;
        __result = ApplyAsync(__instance, target, props, dealer, cardSource);
        return false;
    }

    private static async Task ApplyAsync(
        PersonalHivePower power,
        Creature target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (power.Owner == null) return;
        if (!props.IsPoweredAttack() && (cardSource?.Type != CardType.Attack)) return;

        if (target.Monster == null && target.PetOwner?.Creature != null)
            dealer = target.PetOwner.Creature;

        var player = dealer?.Player;
        if (player == null) return;

        var combat = power.CombatState;
        if (combat == null) return;

        var count = power.Amount;
        if (count <= 0) return;

        var cards = new List<CardModel>(count);
        for (var i = 0; i < count; i++)
            cards.Add(combat.CreateCard(ModelDb.Card<Dazed>(), player));

        var results = await CombatCardPilePreview.AddGeneratedCardsSilentAsync(
            cards, PileType.Draw, player);
        PendingDrawPileCardPreview.Enqueue(results);
    }
}

/// <summary>攻撃カード完了後に山札プレビューを処理する。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class CardPileAddPreviewFlushPatch
{
    public static void Postfix(ref Task __result, CardPlay cardPlay)
    {
        var original = __result;
        __result = ContinueAsync(original, cardPlay);
    }

    private static async Task ContinueAsync(Task original, CardPlay cardPlay)
    {
        await original;
        await OtherColorFetishResolver.TryResolveAfterCardPlayedAsync(cardPlay);
        if (cardPlay.Card.Type == CardType.Attack)
            await PendingDrawPileCardPreview.FlushIfAnyAsync();
    }
}

/// <summary>
/// 手札遅延追加は効果ラッパー完了後にフラッシュする。
/// カード: OnPlayWrapper 終了後（廃棄演出後）。
/// ポーション: OnUseWrapper 終了後（スキルポーション等の本家手札追加を含む）。
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
public static class PendingHandCardAddOnPlayWrapperFlushPatch
{
    public static void Postfix(ref Task __result) =>
        __result = PendingHandCardAddFlush.AfterAsync(__result);
}

[HarmonyPatch(typeof(PotionModel), nameof(PotionModel.OnUseWrapper))]
public static class PendingHandCardAddOnPotionUseWrapperFlushPatch
{
    public static void Postfix(ref Task __result) =>
        __result = PendingHandCardAddFlush.AfterAsync(__result);
}

/// <summary>カード／ポーション効果ラッパー終了後の手札遅延フラッシュ。</summary>
internal static class PendingHandCardAddFlush
{
    public static async Task AfterAsync(Task original)
    {
        await original;
        await PendingHandCardAdd.FlushIfAnyAsync();
        await PendingStatusHypnosisConvert.FlushIfAnyAsync();
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
public static class DrawPileCardAddPreviewTurnStartPatch
{
    public static void Postfix(ref Task __result)
    {
        var original = __result;
        __result = ContinueAsync(original);
    }

    private static async Task ContinueAsync(Task original)
    {
        await original;
        await PendingStatusHypnosisConvert.FlushIfAnyAsync();
        PendingDrawPileCardPreview.Clear();
        PendingHandCardAdd.Clear();
        PendingStatusHypnosisConvert.Clear();
    }
}
