using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>デジタルサイン — カウント軸のコスト操作。手札のカードコストをこの戦闘中 -1。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class DigitalSign() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Cards", 2M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var hand = Owner.PlayerCombatState.Hand.Cards
            .Where(c => c != this)
            .Take(DynamicVars["Cards"].IntValue)
            .ToList();

        foreach (var card in hand)
            card.EnergyCost.AddThisCombat(-1);

        await Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1M);
}
