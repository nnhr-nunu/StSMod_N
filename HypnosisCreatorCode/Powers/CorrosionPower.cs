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
/// アップグレード後は、プレイしたアタックの性癖タグと一致するカウントカードを優先する。
/// </summary>
public class CorrosionPower : HypnosisCreatorPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool PreferMatchingFetish { get; set; }

    private static bool IsCountCandidate(CardModel c) =>
        c is HypnosisCreatorCard { Rarity: not CardRarity.Token } hc &&
        hc.Keywords.Contains(HypnosisCreatorCode.CustomEnums.HcKeywords.Count) &&
        c is not TrainingCommand;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null || !Owner.IsAlive) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (cardPlay.Card.Type != CardType.Attack) return;

        var pool = ModelDb.AllCards.Where(IsCountCandidate).ToList();
        if (pool.Count == 0) return;

        var player = Owner.Player;
        if (player == null) return;

        var rng = player.RunState.Rng.CombatCardSelection;

        CardModel canonical;
        var sourceFetishes = CardFetishLookup.GetFetishes(cardPlay.Card);
        if (PreferMatchingFetish && sourceFetishes.Count > 0)
        {
            var matching = pool
                .Where(c => CardFetishLookup.GetFetishes(c).Any(f => sourceFetishes.Contains(f)))
                .ToList();
            canonical = matching.Count > 0
                ? matching[rng.NextInt(matching.Count)]
                : pool[rng.NextInt(pool.Count)];
        }
        else
        {
            canonical = pool[rng.NextInt(pool.Count)];
        }

        var generated = CombatState!.CreateCard(canonical, player);
        await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, player);
    }
}
