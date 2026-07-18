using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Uncommon;

/// <summary>オーバーフォーカス — 対象の性癖スロットを増やし、ランダムな性癖を1つ目覚めさせる。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class OverFocus() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Slots", 1M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        EnemyFetishSlots.AddCapacity(play.Target, DynamicVars["Slots"].IntValue);
        EnemyFetishSlots.TryPlantRandom(play.Target, Owner, Owner.RunState.Rng.CombatCardSelection);
        await Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars["Slots"].UpgradeValueBy(1M);
}
