using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>儀式の開示 — 対象のランダムな性癖を1つ目覚めさせ、破滅5を付与する。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class RitualReveal() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Doom", 5M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        EnemyFetishSlots.AddCapacity(play.Target, 1);
        EnemyFetishSlots.TryPlantRandom(play.Target, Owner, Owner.RunState.Rng.CombatCardSelection);
        await FetishCombat.ApplyDoom(
            choiceContext, play.Target, DynamicVars["Doom"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Doom"].UpgradeValueBy(5M);
}
