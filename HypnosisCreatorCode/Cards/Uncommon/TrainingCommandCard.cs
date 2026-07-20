using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Cards.Token;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>調教コマンド — DomSub。ランダムな調教命令カードを3枚（UGで5枚）手札に加えて廃棄する。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class TrainingCommandCard() : HypnosisCreatorCard(2,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.Self)
{
    public override IReadOnlyList<FetishType> CardFetishes => [FetishType.DomSub];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CardsVar(3)];

    private static bool IsCommandCard(CardModel c) => c is TrainingCommand;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (CombatState == null) return;

        var pool = ModelDb.AllCards.Where(IsCommandCard).ToList();
        if (pool.Count == 0) return;

        var rng = Owner.RunState.Rng.CombatCardSelection;
        var generated = new List<CardModel>(DynamicVars.Cards.IntValue);
        for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var canonical = pool[rng.NextInt(pool.Count)];
            generated.Add(CombatState.CreateCard(canonical, Owner));
        }

        await TrainingCommand.AddGeneratedToHandOrderedAsync(generated, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(2M);
}
