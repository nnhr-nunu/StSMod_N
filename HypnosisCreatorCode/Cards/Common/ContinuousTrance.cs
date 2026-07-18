using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>連続トランス — トランス1を3回付与し性癖にトランスを追加。UGで相手すべてに同効果。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class ContinuousTrance() : HypnosisCreatorCard(0,
    CardType.Skill, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Times", 3M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var times = DynamicVars["Times"].IntValue;

        if (IsUpgraded && CombatState != null)
        {
            foreach (var enemy in CombatState.HittableEnemies.ToList())
            {
                FetishCombat.Awaken(enemy, FetishType.Trance, Owner);
                await TranceCombat.ApplyTranceRepeated(
                    choiceContext, enemy, times, Owner.Creature, this);
            }
            return;
        }

        ArgumentNullException.ThrowIfNull(play.Target);
        FetishCombat.Awaken(play.Target, FetishType.Trance, Owner);
        await TranceCombat.ApplyTranceRepeated(
            choiceContext, play.Target, times, Owner.Creature, this);
    }

    protected override void OnUpgrade() { }
}
