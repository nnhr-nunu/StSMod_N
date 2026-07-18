using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>
/// ぜんぶ知ってるよ — 性癖刺さり破滅を2倍にする。UGで天賦。
/// </summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class KnowItAll() : HypnosisCreatorCard(1,
    CardType.Power, CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Multiplier", 1M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        FetishCombat.FetishHitMultiplier += DynamicVars["Multiplier"].BaseValue;
        await Task.CompletedTask;
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}
