using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 教祖化 — 有効な間 SM・DomSub・アブノーマルの性癖カードは必ず刺さる（トランス性癖は対象外）。
/// ターン開始時、ランダムな催眠系カウントカードを Amount 枚手札に加える。
/// トランス付与時のエナジー・ドローは <see cref="TranceCombat"/> 側から通知。
/// </summary>
public class CultLeaderPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private static bool IsCountCandidate(CardModel c) =>
        c is HypnosisCreatorCard { Rarity: not CardRarity.Token } hc &&
        hc.Keywords.Contains(HypnosisCreatorCode.CustomEnums.HcKeywords.Count) &&
        c is not TrainingCommand;

    public override Task AfterApplied(Creature? applier, MegaCrit.Sts2.Core.Models.CardModel? cardSource)
    {
        FetishCombat.CultLeaderActive = true;
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        FetishCombat.CultLeaderActive = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (Owner == null || player.Creature != Owner || CombatState == null) return;

        var count = Math.Max(1, Amount);
        var pool = ModelDb.AllCards.Where(IsCountCandidate).ToList();
        if (pool.Count == 0) return;

        var rng = player.RunState.Rng.CombatCardSelection;
        for (var i = 0; i < count; i++)
        {
            var canonical = pool[rng.NextInt(pool.Count)];
            var generated = CombatState.CreateCard(canonical, player);
            await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, player);
        }
    }
}
