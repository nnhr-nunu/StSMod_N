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
        __result = ApplyAsync(__instance, target, props, dealer);
        return false;
    }

    private static async Task ApplyAsync(
        PersonalHivePower power,
        Creature target,
        ValueProp props,
        Creature? dealer)
    {
        if (power.Owner == null) return;
        if (!props.IsPoweredAttack()) return;

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

/// <summary>攻撃カードの処理完了後、遅延キューに溜まった山札追加プレビューを表示する。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
public static class DrawPileCardAddPreviewFlushPatch
{
    public static void Postfix(ref Task __result, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack) return;

        var original = __result;
        __result = ContinueAsync(original);
    }

    private static async Task ContinueAsync(Task original)
    {
        await original;
        await PendingDrawPileCardPreview.FlushIfAnyAsync();
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterPlayerTurnStart))]
public static class DrawPileCardAddPreviewTurnStartPatch
{
    public static void Postfix() => PendingDrawPileCardPreview.Clear();
}
