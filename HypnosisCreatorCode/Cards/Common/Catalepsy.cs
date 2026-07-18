using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>カタレプシー — スロウ相当の debuff を付与する。UGで、対象がトランス中ならリセットされなくなる。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class Catalepsy() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Slow", 3M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await PowerCmd.Apply<CatalepsyPower>(
            choiceContext, play.Target, DynamicVars["Slow"].BaseValue, Owner.Creature, this);

        if (IsUpgraded)
        {
            var power = play.Target.GetPower<CatalepsyPower>();
            if (power != null) power.PersistIfTranced = true;
        }
    }

    protected override void OnUpgrade() { }
}
