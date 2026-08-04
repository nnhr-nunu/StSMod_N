using HypnosisCreator.HypnosisCreatorCode.Cards;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Powers;

/// <summary>
/// 侵食 — アタックカードをプレイするたび、ランダムな催眠カウントカードを手札に加える。
/// </summary>
public class CorrosionPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private static bool IsCountCandidate(CardModel c) =>
        c is HypnosisCreatorCard { Rarity: not CardRarity.Token } &&
        CountRules.HasCountKeyword(c) &&
        c is not TrainingCommand;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (MassHypnosisPower.IsPropagating) return;
        if (cardPlay.Card.Type != CardType.Attack) return;

        var pool = ModelDb.AllCards.Where(IsCountCandidate).ToList();
        if (pool.Count == 0) return;

        var player = Owner.Player;
        if (player == null) return;

        var rng = player.RunState.Rng.CombatCardSelection;

        var canonical = pool[rng.NextInt(pool.Count)];

        var generated = CombatState!.CreateCard(canonical, player);
        await CombatCardPilePreview.AddGeneratedCardAsync(generated, PileType.Hand, player);
    }
}
