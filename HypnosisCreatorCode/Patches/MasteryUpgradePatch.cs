using HarmonyLib;
using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Patches;

/// <summary>戦闘終了時: 練達が有効なプレイヤーの、戦闘中にプレイしたカウントカードを永続アップグレードする。</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCombatEnd))]
public static class MasteryUpgradePatch
{
    public static void Postfix(IRunState runState, ICombatState? combatState, CombatRoom room)
    {
        if (combatState == null) return;

        foreach (var player in combatState.Players)
        {
            var mastery = player.Creature.GetPower<MasteryPower>();
            if (mastery == null) continue;

            foreach (var card in mastery.PlayedCountCards.ToList())
            {
                if (!card.IsUpgraded)
                    CardCmd.Upgrade(card);
            }

            if (mastery.AddHypnosisCountCardReward)
                AddHypnosisCountCardReward(room, player);
        }
    }

    private static void AddHypnosisCountCardReward(CombatRoom room, Player player)
    {
        var pool = ModelDb.AllCards
            .Where(card =>
                card is HypnosisCreatorCard { Rarity: not CardRarity.Token }
                && CountRules.HasCountKeyword(card)
                && card is not TrainingCommand)
            .ToList();
        if (pool.Count == 0) return;

        var rng = player.RunState.Rng.CombatCardSelection;
        var cards = pool
            .OrderBy(_ => rng.NextInt(int.MaxValue))
            .Take(3)
            .Select(card => card.CreateCloneForPlayer(player))
            .ToList();
        if (cards.Count == 0) return;

        room.AddExtraReward(
            player,
            new CardReward(cards, CardCreationSource.Other, player, null!, null!));
    }
}
