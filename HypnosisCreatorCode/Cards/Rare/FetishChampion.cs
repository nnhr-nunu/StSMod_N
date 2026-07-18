using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Rare;

/// <summary>性癖チャンピオン — 対象の性癖を全種目覚めさせ、破滅を付与する。UGでブロックも得る。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class FetishChampion() : HypnosisCreatorCard(3,
    CardType.Attack, CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Doom", 8M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        EnemyFetishSlots.AddCapacity(play.Target, 4);
        FetishCombat.AwakenAll(play.Target, Owner);
        await FetishCombat.ApplyDoom(
            choiceContext, play.Target, DynamicVars["Doom"].IntValue, Owner.Creature, this);

        if (IsUpgraded)
            await CreatureCmd.GainBlock(Owner.Creature, 6M, ValueProp.Move, play);
    }

    protected override void OnUpgrade() => DynamicVars["Doom"].UpgradeValueBy(4M);
}
