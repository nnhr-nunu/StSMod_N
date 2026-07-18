using BaseLib.Utils;
using HypnosisCreator.HypnosisCreatorCode.Character;
using HypnosisCreator.HypnosisCreatorCode.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HypnosisCreator.HypnosisCreatorCode.Cards.Common;

/// <summary>連続トランス — トランス1を3回付与する（トランス性癖は毎回刺さる）。</summary>
[Pool(typeof(HypnosisCreatorCardPool))]
public class ContinuousTrance() : HypnosisCreatorCard(1,
    CardType.Skill, CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Times", 3M)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        // トランス性癖は付与のたびに刺さるため、先に目覚めさせてから付与を繰り返す。
        FetishCombat.Awaken(play.Target, FetishType.Trance, Owner);
        await TranceCombat.ApplyTranceRepeated(
            choiceContext, play.Target, DynamicVars["Times"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Times"].UpgradeValueBy(1M);
}
