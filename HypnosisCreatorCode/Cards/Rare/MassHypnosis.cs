using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// 集団催眠 — パワー。催眠系カウントカードを相手全体へ波及させ、ランダムな3枚から1枚を手札に加える。
/// UGで天賦。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class MassHypnosis() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Uncommon,
    TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<MassHypnosisPower>(
            choiceContext, Owner.Creature, 1M, Owner.Creature, this);

        if (CombatState == null) return;

        var pool = ModelDb.AllCards
            .Where(card =>
                card is HypnosisCreatorCard { Rarity: not CardRarity.Token }
                && CountRules.HasCountKeyword(card)
                && card is not TrainingCommand)
            .ToList();
        if (pool.Count == 0) return;

        var rng = Owner.RunState.Rng.CombatCardSelection;
        var options = pool
            .OrderBy(_ => rng.NextInt(int.MaxValue))
            .Take(3)
            .Select(card => CombatState.CreateCard(card, Owner))
            .ToList();

        var selected = await CardSelectCmd.FromChooseACardScreen(
            choiceContext, options, Owner, canSkip: false);
        var chosen = selected ?? options[rng.NextInt(options.Count)];
        await CombatCardPilePreview.AddGeneratedCardAsync(chosen, PileType.Hand, Owner);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}
